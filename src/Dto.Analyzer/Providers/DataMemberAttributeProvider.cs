using System.Composition;
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
  }
}
