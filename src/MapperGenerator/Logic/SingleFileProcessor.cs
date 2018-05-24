using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using DtoGenerator.Logic.Interfaces;
using DtoGenerator.Logic.Models;
using DtoGenerator.Logic.PropertyMappers;

namespace DtoGenerator.Logic
{
  public class SingleFileProcessor : ISingleFileProcessor
  {
    public async Task<FileInfo> Analyze(
      string selectedFilePath,
      IEnumerable<string> allProjectSourcesExceptSelected,
      Action<int> onProgressChanged)
    {
      int[] progress = {0};
      var projectSourcesParsingTasks = allProjectSourcesExceptSelected.Select(path => Task.Run(async () =>
      {
        var sourceCode = await Helper.ReadFile(path);
        onProgressChanged?.Invoke(++progress[0]);
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        onProgressChanged?.Invoke(++progress[0]);
        return tree;
      }));
      var projectSyntaxTrees = await Task.WhenAll(projectSourcesParsingTasks);

      var selectedFileContent = await Helper.ReadFile(selectedFilePath);
      var selectedSyntaxTree = CSharpSyntaxTree.ParseText(selectedFileContent);
      var syntaxRoot = await selectedSyntaxTree.GetRootAsync();
      var namespaceTitle = syntaxRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();
      var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

      var compilation = CSharpCompilation.Create(namespaceTitle)
        .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
        .AddSyntaxTrees(projectSyntaxTrees.Union(new[] {selectedSyntaxTree}));
      var semanticModel = compilation.GetSemanticModel(selectedSyntaxTree);

      return new FileInfo
      {
        Namespace = $"{namespaceTitle}.{Constants.DtoSuffix}",
        Classes = classes.Select(classNode => new ClassInfo
        {
          Name = classNode.Identifier.ValueText,
          Properties = classNode.DescendantNodes()
                        .OfType<PropertyDeclarationSyntax>()
                        .Select(prop => getPropertyInfo(prop, semanticModel)).ToList()
        }).ToList()
      };
    }

    private PropertyInfo getPropertyInfo(PropertyDeclarationSyntax node, SemanticModel semanticModel)
    {
      var isEnumerable = false;
      var isGeneric = false;
      var typeInfo = semanticModel.GetTypeInfo(node.Type);

      var propertyName = node.Identifier.ValueText;
      var hasSetter = node.AccessorList?.Accessors.FirstOrDefault(
                        a => a.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                             !a.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) != null;
      var typeName = "";
      PropertyMapper mapper = new UnknownTypePropertyMapper(propertyName, hasSetter);
      if (typeInfo.ConvertedType is INamedTypeSymbol namedType)
      {
        var namedTypeFullName = namedType.ToString().Replace(" ", string.Empty);
        isEnumerable = namedType.ConstructedFrom.MemberNames.Contains("GetEnumerator");
        var simpleTypeKinds = new[] { TypeKind.Enum, TypeKind.Struct, TypeKind.Interface };
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
        Type = node.Type,
        TypeName = typeName,
        IsEnumerableType = isEnumerable,
        IsGenericType = isGeneric,
        Mapper = mapper,
        HasSetter = hasSetter
      };
    }
  }
}
