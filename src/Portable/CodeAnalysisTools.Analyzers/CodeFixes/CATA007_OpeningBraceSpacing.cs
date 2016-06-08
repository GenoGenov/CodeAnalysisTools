using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalysisTools.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OpeningBraceSpacingCodeFixProvider)), Shared]
	public class OpeningBraceSpacingCodeFixProvider : CodeFixProvider
	{
		private const string title = "Fix brace spacing.";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CATA007"); }
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

			var declaration = root.FindToken(diagnosticSpan.Start);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					c => FixBraceSpacingAsync(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> FixBraceSpacingAsync(Document document, SyntaxToken declaration, CancellationToken cancellationToken)
		{
			var newDeclaration = declaration
				.WithTrailingTrivia(declaration.TrailingTrivia.Add(SyntaxFactory.Space));

			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.ReplaceToken(declaration, newDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}