using System.Linq;
using System.Threading.Tasks;
using CodeAnalysisTools.Options;
using CodeAnalysisTools.Providers.CodeActions;
using CodeAnalysisTools.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Refactorings
{
	public class FixNamespaceCodeRefactoringProvider : CodeAnalysisRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, CodeAnalysisOptions options)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);

			var namespaceDeclaration = this.GetNameSpaceDeclaration(node);

			if (namespaceDeclaration != null)
			{
				context.RegisterRefactoring(new FixNamespaceCodeAction(context.Document, namespaceDeclaration));
			}
		}

		private NamespaceDeclarationSyntax GetNameSpaceDeclaration(SyntaxNode node)
		{
			while (node != null)
			{
				switch (node.Kind())
				{
					case SyntaxKind.NamespaceDeclaration:
						return node as NamespaceDeclarationSyntax;
					case SyntaxKind.NamespaceKeyword:
						return node.AncestorsAndSelf().OfType<NamespaceDeclarationSyntax>().First();
					case SyntaxKind.IdentifierName:
					case SyntaxKind.QualifiedName:
						node = node.Parent;
						break;
					default:
						return null;
				}
			}

			return null;
		}
	}
}