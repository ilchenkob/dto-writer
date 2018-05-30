using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dto.Analyzer.Providers
{
  [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(DataMemberAttributeProvider)), Shared]
  public class DataMemberAttributeProvider : BasePropertyAttributeProvider
  {
    protected override string AttributeName => Constants.Attribute.DataMember;

    protected override PropertyDeclarationSyntax AddAttribute(PropertyDeclarationSyntax prop)
    {
      return prop.AddDataMemberAttribute();
    }

    protected override SyntaxNode OnPropertyAttributeAdded(SyntaxNode root, ClassDeclarationSyntax classDeclaration)
    {
      if (!classDeclaration.HasAttribute(Constants.Attribute.DataContract))
      {
        return root.ReplaceNode(classDeclaration, classDeclaration.AddDataContractAttribute());
      }

      return root;
    }

    protected override SyntaxNode OnPropertyAttributeRemoved(SyntaxNode root, ClassDeclarationSyntax classDeclaration)
    {
      if (classDeclaration.HasAttribute(Constants.Attribute.DataContract) &&
          classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().All(p => !p.HasAttribute(AttributeName)))
      {
        return root.ReplaceNode(classDeclaration, classDeclaration.RemoveAttribute(Constants.Attribute.DataContract));
      }
      return root;
    }
  }
}
