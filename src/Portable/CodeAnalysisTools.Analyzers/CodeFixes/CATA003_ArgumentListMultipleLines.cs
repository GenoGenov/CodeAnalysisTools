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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;

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
			var tabSize = document.Project.Solution.Workspace.Options.GetOption(FormattingOptions.TabSize, LanguageNames.CSharp);
			bool useTabs = document.Project.Solution.Workspace.Options.GetOption(FormattingOptions.UseTabs, LanguageNames.CSharp);

			var root = await document.GetSyntaxRootAsync();

			var newSyntax = syntax.ReplaceToken(
											   syntax.OpenParenToken,
											   syntax.OpenParenToken
					.WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine)));


			newSyntax = newSyntax.ReplaceTokens(
												newSyntax.Arguments.GetSeparators(),
												(oldNode, newNode) =>
												{
												  if (oldNode.IsKind(SyntaxKind.CommaToken))
												  {
													  return oldNode.WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine)).WithLeadingTrivia();
												  }
												  return oldNode;
				  });


			newSyntax = newSyntax.ReplaceNodes(
											  newSyntax.Arguments,
											  (oldNode, newNode) =>
											  	{
											  		if (oldNode.IsKind(SyntaxKind.Argument))
											  		{
														  var argumentTrivia = IndentationHelper.GetIndentationTriviaByNode(useTabs, tabSize, root, syntax.OpenParenToken, cancellationToken);
														  return IndentationHelper.FormatNodeRecursive(oldNode, argumentTrivia);
											  		}
											  		return oldNode;
											  	});


			var newRoot = root.ReplaceNode(syntax, newSyntax);
			var newDoc = document.WithSyntaxRoot(newRoot);
			return newDoc;
		}
	}
}