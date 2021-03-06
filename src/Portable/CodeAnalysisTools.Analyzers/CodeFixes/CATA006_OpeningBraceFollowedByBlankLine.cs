﻿using System.Collections.Immutable;
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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OpeningBraceFollowedByBlankLineCodeFixProvider)), Shared]
	public class OpeningBraceFollowedByBlankLineCodeFixProvider : CodeFixProvider
	{
		private const string title = "Remove trailing blank lines.";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CATA006"); }
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
					c => RemoveTrailingBlankLineAsync(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> RemoveTrailingBlankLineAsync(Document document, SyntaxToken token, CancellationToken cancellationToken)
		{
			var newNode = token.RemoveFollowingBlankLine();

			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.ReplaceNode(token.Parent, newNode);

			// Return the new solution with the now-uppercase type name.
			return document.WithSyntaxRoot(newRoot);
		}
	}
}