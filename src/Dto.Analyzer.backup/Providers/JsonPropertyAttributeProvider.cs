using System.Composition;
using Dto.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dto.Analyzer.Providers
{
  [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(JsonPropertyAttributeProvider)), Shared]
  public class JsonPropertyAttributeProvider : BasePropertyAttributeProvider
  {
    protected override string AttributeName => Constants.Attribute.JsonProperty;

    protected override PropertyDeclarationSyntax AddAttribute(PropertyDeclarationSyntax prop)
    {
      return prop.AddJsonPropertyAttribute();
    }
  }
}
