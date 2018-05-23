using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DtoGenerator.Logic.Models;
using DtoGenerator.Logic.PropertyMappers;

namespace DtoGenerator.Logic
{
  public class SingleFileProcessor
  {
    public async Task<FileInfo> Analyze(string selectedFilePath, IEnumerable<string> allProjectSourcesExceptSelected, Action<int> onProgressChanged)
    {
      var progress = 0;
      var projectSourcesParsingTasks = allProjectSourcesExceptSelected.Select(path => Task.Run(async () =>
      {
        var sourceCode = await Helper.ReadFile(path);
        onProgressChanged?.Invoke(++progress);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        onProgressChanged?.Invoke(++progress);
        return syntaxTree;
      }));
      var projectSyntaxTrees = await Task.WhenAll(projectSourcesParsingTasks);

      var selectedFileContent = await Helper.ReadFile(selectedFilePath);
      var selectedSyntaxTree = CSharpSyntaxTree.ParseText(selectedFileContent);
      var syntaxRoot = await selectedSyntaxTree.GetRootAsync();
      var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
      var namespaceTitle = syntaxRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();
      
      var compilation = CSharpCompilation.Create(namespaceTitle)
                      .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                      .AddSyntaxTrees(projectSyntaxTrees.Union(new []{ selectedSyntaxTree }));
      var semanticModel = compilation.GetSemanticModel(selectedSyntaxTree);

      return new FileInfo
      {
        Namespace = $"{namespaceTitle}.{Constants.DtoSuffix}",
        Classes = classes.Select(item => new ClassInfo
        {
          Name = item.Identifier.ValueText,
          Properties = item.Members
                            .OfType<PropertyDeclarationSyntax>()
                            .Select(p => getPropertyInfo(semanticModel, p))
                            .ToList()
        }).ToList()
      };
    }

    public string GenerateSourcecode(FileInfo fileInfo)
    {
      var dtoNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{fileInfo.Namespace}"));
      foreach (var model in fileInfo.Classes.Where(c => c.IsEnabled))
      {
        var dtoName = $"{model.Name}{Constants.DtoSuffix}";
        var dtoDeclaration = SyntaxFactory.ClassDeclaration(dtoName)
                                          .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var fromModelFieldsMapping = new StringBuilder("\n");
        var toModelFieldsMapping = new StringBuilder("\n");
        foreach (var prop in model.Properties.Where(p => p.IsEnabled))
        {
          var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
          if (!prop.HasSetter)
            setAccessor = setAccessor.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

          var propertyDeclaration = SyntaxFactory
                  .PropertyDeclaration(string.IsNullOrWhiteSpace(prop.TypeName) ? prop.Type : SyntaxFactory.ParseTypeName(prop.TypeName), prop.Name)
                  .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                  .AddAccessorListAccessors(
                      SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                      setAccessor);
            
          dtoDeclaration = dtoDeclaration.AddMembers(propertyDeclaration);

          if (model.NeedFromModelMethod)
            fromModelFieldsMapping.Append(prop.Mapper.GetFromModelMappingSyntax());
          if (model.NeedToModelMethod)
            toModelFieldsMapping.Append(prop.Mapper.GetToModelMappingSyntax());
        }
        
        if (model.NeedFromModelMethod)
          dtoDeclaration = dtoDeclaration.AddMembers(createFromModelMethodDeclarationSyntax(fromModelFieldsMapping.ToString(), dtoName, model.Name));
        if (model.NeedToModelMethod)
          dtoDeclaration = dtoDeclaration.AddMembers(createToModelMethodDeclarationSyntax(toModelFieldsMapping.ToString(), model.Name));

        dtoNamespace = dtoNamespace.AddMembers(dtoDeclaration);
      }

      var dtoFile = SyntaxFactory.CompilationUnit()
                                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(Constants.Using.System)))
                                .AddMembers(dtoNamespace);

      if (fileInfo.Classes.Where(c => c.IsEnabled)
                          .Any(c => c.Properties.Any(p => p.IsGenericType && p.IsEnabled)))
      {
        dtoFile = dtoFile.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(Constants.Using.SystemCollectionsGeneric)));
      }
      if (fileInfo.Classes.Where(c => c.IsEnabled)
        .Any(c => c.Properties.Any(p => p.IsEnumerableType && p.IsEnabled)))
      {
        dtoFile = dtoFile.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(Constants.Using.SystemLinq)));
      }

      return cleanupCodeFormatting(dtoFile.NormalizeWhitespace().ToFullString());
    }

    private PropertyInfo getPropertyInfo(SemanticModel semanticModel, PropertyDeclarationSyntax p)
    {
      var isEnumerable = false;
      var isGeneric = false;
      var typeInfo = semanticModel.GetTypeInfo(p.Type);

      var propertyName = p.Identifier.ValueText;
      var hasSetter = p.AccessorList?.Accessors.FirstOrDefault(
                        a => a.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                             !a.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) != null;
      var typeName = "";
      PropertyMapper mapper = new UnknownTypePropertyMapper(propertyName, hasSetter);
      if (typeInfo.ConvertedType is INamedTypeSymbol namedType)
      {
        var namedTypeFullName = namedType.ToString().Replace(" ", string.Empty);
        isEnumerable = namedType.ConstructedFrom.MemberNames.Contains("GetEnumerator");
        var simpleTypeKinds = new[] {TypeKind.Enum, TypeKind.Struct, TypeKind.Interface};
        var isSimple = namedType.IsValueType ||
                   simpleTypeKinds.Any(t => namedType.TypeKind == t) ||
                   namedType.Name.Equals("object", StringComparison.CurrentCultureIgnoreCase) ||
                   namedType.Name.Equals("string", StringComparison.CurrentCultureIgnoreCase);

        if (isSimple)
        {
          mapper = new SimplePropertyMapper(propertyName, hasSetter);
        }
        else
        {
          isGeneric = namedType.ConstructedFrom.IsGenericType;
          if (isEnumerable)
          {
            if (namedType.Name.Equals("List", StringComparison.InvariantCultureIgnoreCase))
            {
              typeName = namedTypeFullName.Substring(namedTypeFullName.LastIndexOf(".List", StringComparison.InvariantCultureIgnoreCase) + 1);
              var fullGenericPart = $"{typeName.Replace("List<", string.Empty).Replace(">", string.Empty)}";
              var genericPart = fullGenericPart.Substring(fullGenericPart.LastIndexOf('.') + 1);

              if (Constants.SimpleTypeNames.Any(t => genericPart.Equals(t)))
              {
                mapper = new SimpleGenericListPropertyMapper(propertyName, hasSetter);
              }
              else
              {
                genericPart = $"{genericPart}{Constants.DtoSuffix}";
                mapper = new ListPropertyMapper(genericPart, propertyName, hasSetter);
              }
              typeName = $"List<{genericPart}>";
            }
          }
          else
          {
            typeName = $"{namedType.Name}{Constants.DtoSuffix}";
            mapper = new ClassPropertyMapper(typeName, propertyName, hasSetter);
          }
        }
      }
      else
      {
        if (typeInfo.ConvertedType.Kind == SymbolKind.ArrayType && typeInfo.ConvertedType is IArrayTypeSymbol arrayType)
        {
          if (arrayType.ElementType.IsValueType || arrayType.ElementType.Name.ToLowerInvariant().Contains("string"))
          {
            mapper = new SimpleArrayPropertyMapper(propertyName, hasSetter);
          }
          else
          {
            typeName = $"{arrayType.ElementType.Name}{Constants.DtoSuffix}[]";
            mapper = new ArrayPropertyMapper($"{arrayType.ElementType.Name}{Constants.DtoSuffix}", propertyName, hasSetter);
          }

          isEnumerable = true;
        }
      }

      return new PropertyInfo
      {
        Name = propertyName,
        Type = p.Type,
        TypeName = typeName,
        IsEnumerableType = isEnumerable,
        IsGenericType = isGeneric,
        Mapper = mapper,
        HasSetter = hasSetter
      };
    }

    private string cleanupCodeFormatting(string code)
    {
      var propertyRegex = new Regex(@"\s*{\r\n\s*get;\r\n\s*set;\r\n\s*}");
      var privatePropertyRegEx = new Regex(@"\s*{\r\n\s*get;\r\n\s*private\sset;\r\n\s*}");
      return privatePropertyRegEx.Replace(propertyRegex.Replace(code, " { get; set; }"), " { get; private set; }")
        .Replace(" ;", ";")
        .Replace(" ,", ",")
        .Replace("}; \r\n\r\n", "}; \r\n")
        .Replace("};\r\n\r\n", "};\r\n");
    }

    private MethodDeclarationSyntax createFromModelMethodDeclarationSyntax(string fromModelFieldsMappingString, string dtoName, string modelName)
    {
      var fromModelMethodBody = fromModelFieldsMappingString.Length > 3 ? $"{{{fromModelFieldsMappingString}}};" : "{ };";
      var fromModelMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(dtoName), PropertyMapper.FromModelMethodName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
        .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(PropertyMapper.FromModelParamName)).WithType(SyntaxFactory.ParseTypeName(modelName)))
        .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement($"return new {dtoName}()"),
          SyntaxFactory.ParseStatement(fromModelMethodBody)));

      return fromModelMethodDeclaration;
    }

    private MethodDeclarationSyntax createToModelMethodDeclarationSyntax(string toModelFieldsMappingString, string modelName)
    {
      var toModelMethodBody = toModelFieldsMappingString.Length > 3 ? $"{{{toModelFieldsMappingString}}};" : "{ };";
      var toModelMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(modelName), PropertyMapper.ToModelMethodName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement($"return new {modelName}()"),
          SyntaxFactory.ParseStatement(toModelMethodBody)));

      return toModelMethodDeclaration;
    }
  }
}
