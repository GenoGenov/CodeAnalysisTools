using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ClosingBracePrecededByBlankLineAnalyzer : SyntaxTreeAnalyzer
	{
		public override string DiagnosticId
		{
			get
			{
				return "CATA002";
			}
		}

		public override string Title
		{
			get
			{
				return "Closing curly bracket is preceeded by a blank line.";
			}
		}

		public override string Description
		{
			get
			{
				return "Closing curly bracket is preceeded by a blank line.";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "Closing curly bracket is preceeded by a blank line.";
			}
		}

		public override string Category
		{
			get
			{
				return "Readability";
			}
		}

		public async override Task AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			var root = await context.Tree.GetRootAsync().ConfigureAwait(false);
			var closingBraces = root.DescendantTokens().Where(x => x.IsKind(SyntaxKind.CloseBraceToken));

			foreach (var brace in closingBraces)
			{
				if (brace.LeadingTrivia.Where(x => x.IsKind(SyntaxKind.EndOfLineTrivia)).Any())
				{
					var diagnostic = Diagnostic.Create(Rule, brace.GetLocation(), string.Empty);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}