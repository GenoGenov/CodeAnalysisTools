using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArgumentListMultipleLinesCodeFixProvider)), Shared]
	public class ArgumentListMultipleLinesCodeFixProvider : CodeFixProvider
	{
		private const string title = "Place arguments on separate lines.";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CATA003"); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var syntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ArgumentListSyntax>().First();

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					c => RemoveLeadingBlankLineAsync(context.Document, syntax, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> RemoveLeadingBlankLineAsync(Document document, ArgumentListSyntax syntax, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync();

			var newSyntax = syntax.ReplaceToken(
											   syntax.OpenParenToken, 
											   syntax.OpenParenToken
					.WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine)));

			newSyntax = newSyntax.ReplaceNodes(
											  newSyntax.Arguments, 
											  (oldNode, newNode) =>
											  	{
											  		if (oldNode.IsKind(SyntaxKind.Argument))
											  		{
											  			var argumentTrivia = IndentationHelper.GetIndentationTriviaByNode(root, syntax.OpenParenToken, cancellationToken);
											  			var result = oldNode.WithLeadingTrivia(argumentTrivia);
											  			var blocks = result.DescendantNodes(x => x.IsKind(SyntaxKind.Block) == false).OfType<BlockSyntax>();
											  			var blockTrivia = argumentTrivia.Add(SyntaxFactory.Tab);
											  			foreach (var block in blocks)
											  			{
											  				result = result.ReplaceNode(block, IndentationHelper.FormatBlockRecursive(block, blockTrivia));
											  			}
											  			return result;
											  		}
											  		return oldNode;
											  	});

			newSyntax = newSyntax
				.ReplaceTokens(
							  newSyntax.Arguments.GetSeparators(), 
							  (oldNode, newNode) =>
							  	{
							  		if (oldNode.IsKind(SyntaxKind.CommaToken))
							  		{
							  			return oldNode
						.WithTrailingTrivia(
							oldNode.TrailingTrivia
							.Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia) == false)
								.ToSyntaxTriviaList().Add(SyntaxFactory.EndOfLine(Environment.NewLine)))
						.WithLeadingTrivia();
							  		}
							  		return oldNode;
							  	});

			var newRoot = root.ReplaceNode(syntax, newSyntax);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}