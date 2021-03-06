﻿using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.Providers.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NameSpaceNameCodeFixProvider)), Shared]
	public class NameSpaceNameCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CATA008"); }
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

			var declaration = root.FindNode(diagnosticSpan).AncestorsAndSelf().First(x => x is NamespaceDeclarationSyntax) as NamespaceDeclarationSyntax;

			context.RegisterCodeFix(
				new FixNamespaceCodeAction(context.Document, declaration),
				diagnostic);
		}
	}
}