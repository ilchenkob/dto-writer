using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dto.Analyzer
{
  [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(JsonPropertyAttributeProvider)), Shared]
  public class JsonPropertyAttributeProvider : CodeRefactoringProvider
  {
    private readonly string addAttributeTitle = $"Add {Constants.Attribute.JsonProperty} attribute";
    private readonly string removeAttributeTitle = $"Remove {Constants.Attribute.JsonProperty} attribute";

    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
      // Some general variables we need for further processing and some basic checks to exit as early as possible.
      var document = context.Document;
      if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
        return;
      var span = context.Span;
      if (!span.IsEmpty)
        return;
      var cancellationToken = context.CancellationToken;
      if (cancellationToken.IsCancellationRequested)
        return;

      var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
      var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
      
      var token = root.FindToken(span.Start);
      if (!token.IsKind(SyntaxKind.IdentifierToken))
      {
        // Not the name identifier -> don't offer a refactoring here
        return;
      }

      if (token.Parent is ClassDeclarationSyntax classDeclaration)
      {
        if (classDeclaration.IsDto() &&
            classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().Any(p => !p.ContainsJsonAttribute()))
        {
          context.RegisterRefactoring(CodeAction.Create(addAttributeTitle,
            t => addJsonAttributesClassAction(t, context.Document, root, classDeclaration)));
        }
        if (classDeclaration.IsDto() &&
            classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().Any(p => p.ContainsJsonAttribute()))
        {
          context.RegisterRefactoring(CodeAction.Create(removeAttributeTitle,
            t => removeJsonAttributesClassAction(t, context.Document, root, classDeclaration)));
        }
      }
      else if (token.Parent is PropertyDeclarationSyntax propertyDeclaration)
      {
        if (propertyDeclaration.Parent is ClassDeclarationSyntax parentClass)
        {
          if (parentClass.IsDto())
          {
            if (propertyDeclaration.ContainsJsonAttribute())
            {
              context.RegisterRefactoring(CodeAction.Create(removeAttributeTitle,
                t => removeJsonAttributesPropertyAction(t, context.Document, root, propertyDeclaration)));
            }
            else
            {
              context.RegisterRefactoring(CodeAction.Create(addAttributeTitle,
                t => addJsonAttributesPropertyAction(t, context.Document, root, propertyDeclaration)));
            }
          }
        }
      }
    }

    private Task<Document> addJsonAttributesClassAction(CancellationToken token, Document document, SyntaxNode root, ClassDeclarationSyntax classDeclaration)
    {
      if (token.IsCancellationRequested)
        return Task.FromResult(document);

      var newRoot = root.ReplaceNodes(
        classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().Where(p => !p.ContainsJsonAttribute()),
        (originalProp, newProp) => originalProp.AddJsonPropertyAttribute());

      return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private Task<Document> removeJsonAttributesClassAction(CancellationToken token, Document document, SyntaxNode root, ClassDeclarationSyntax classDeclaration)
    {
      if (token.IsCancellationRequested)
        return Task.FromResult(document);

      var newRoot = root.ReplaceNodes(
        classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>(),
        (originalProp, newProp) => originalProp.RemoveJsonPropertyAttribute());

      return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private Task<Document> addJsonAttributesPropertyAction(CancellationToken token, Document document, SyntaxNode root, PropertyDeclarationSyntax propertyDeclaration)
    {
      if (token.IsCancellationRequested)
        return Task.FromResult(document);

      var newRoot = root.ReplaceNode(propertyDeclaration, propertyDeclaration.AddJsonPropertyAttribute());
      return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private Task<Document> removeJsonAttributesPropertyAction(CancellationToken token, Document document, SyntaxNode root, PropertyDeclarationSyntax propertyDeclaration)
    {
      if (token.IsCancellationRequested)
        return Task.FromResult(document);

      var newRoot = root.ReplaceNode(propertyDeclaration, propertyDeclaration.RemoveJsonPropertyAttribute());
      return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
  }
}
