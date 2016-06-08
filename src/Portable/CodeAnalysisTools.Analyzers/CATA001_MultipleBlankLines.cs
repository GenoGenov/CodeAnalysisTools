using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MultipleBlankLinesAnalyzer : SyntaxTreeAnalyzer
	{
		public override string DiagnosticId
		{
			get
			{
				return "CATA001";
			}
		}

		public override string Title
		{
			get
			{
				return "The code contains multiple blank lines in a row.";
			}
		}

		public override string Description
		{
			get
			{
				return "The code contains multiple blank lines in a row.";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "The code contains multiple blank lines in a row.";
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
			if (context.Tree.IsGenerated())
			{
				return;
			}

			var root = await context.Tree.GetRootAsync();

			var tokens = root.DescendantTokens();

			foreach (var token in tokens)
			{
				if (token.LeadingTrivia.HasConsecutiveEndLineTrivia() || token.TrailingTrivia.HasConsecutiveEndLineTrivia())
				{
					var diagnostic = Diagnostic.Create(Rule, token.GetLocation(), string.Empty);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}
