using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SingleLineCommentFollowedByBlankLineAnalyzer : SyntaxTreeAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(Rule);
			}
		}

		public override string DiagnosticId
		{
			get
			{
				return "CATA005";
			}
		}

		public override string Title
		{
			get
			{
				return "Single line comment must not be followed by a blank line.";
			}
		}

		public override string Description
		{
			get
			{
				return "Single line comment must not be followed by a blank line.";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "Single line comment must not be followed by a blank line.";
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

			var commentTrivias = root.DescendantTrivia().Where(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia)).Distinct();

			foreach (var commentTrivia in commentTrivias)
			{
				bool invalidLeading = commentTrivia.Token.LeadingTrivia.Any(SyntaxKind.SingleLineCommentTrivia) && HasTrailingBlankLine(commentTrivia.Token.LeadingTrivia);
				bool invalidTrailing = commentTrivia.Token.TrailingTrivia.Any(SyntaxKind.SingleLineCommentTrivia) && HasTrailingBlankLine(commentTrivia.Token.TrailingTrivia);

				if (invalidLeading || invalidTrailing)
				{
					var diagnostic = Diagnostic.Create(Rule, commentTrivia.GetLocation(), string.Empty);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}

		private static bool HasTrailingBlankLine(SyntaxTriviaList triviaList)
		{
			var index = 0;

			var lastCommentIndex = -1;

			while (index < triviaList.Count)
			{
				switch (triviaList[index].Kind())
				{
					case SyntaxKind.SingleLineCommentTrivia:
						lastCommentIndex = index;
						break;
					case SyntaxKind.EndOfLineTrivia:
						if (lastCommentIndex == -1)
						{
							break;
						}

						// end of line of the previous comment
						if (lastCommentIndex == index - 1)
						{
							break;
						}
						else if (index > lastCommentIndex)
						{
							return true;
						}
						break;
				}

				index++;
			}

			return false;
		}
	}
}
