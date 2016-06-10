using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CodeAnalysisTools.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SingleLineCommentSpacingCodeFixProvider)), Shared]
	public class SingleLineCommentSpacingCodeFixProvider : CodeFixProvider
	{
		private const string title = "Fix comment spacing";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(
					"CATA004",
					"CATA005");
			}
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

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start);

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					c => FixCommentSpacingAsync(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> FixCommentSpacingAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
		{
			var newDeclaration = token
				.WithLeadingTrivia(TriviaHelper.FixSingleLineCommentSpacing(token.LeadingTrivia))
				.WithTrailingTrivia(TriviaHelper.FixSingleLineCommentSpacing(token.TrailingTrivia));

			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.ReplaceToken(token, newDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}