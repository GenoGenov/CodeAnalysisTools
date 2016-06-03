using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SingleLineCommentPreceededByBlankLineAnalyzer : SyntaxTreeAnalyzer
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
				return "CATA004";
			}
		}

		public override string Title
		{
			get
			{
				return "Single line comment must be preceeded by a blank line.";
			}
		}

		public override string Description
		{
			get
			{
				return "Single line comment must be preceeded by a blank line.";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "Single line comment must be preceeded by a blank line.";
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
				if (commentTrivia.Token.GetPreviousToken().IsKind(SyntaxKind.OpenBraceToken))
				{
					continue;
				}

				bool invalidLeading = commentTrivia.Token.LeadingTrivia.Any(SyntaxKind.SingleLineCommentTrivia) && HasPreceedingBlankLine(commentTrivia.Token.LeadingTrivia) == false;
				bool invalidTrailing = commentTrivia.Token.TrailingTrivia.Any(SyntaxKind.SingleLineCommentTrivia) && HasPreceedingBlankLine(commentTrivia.Token.TrailingTrivia) == false;

				if (invalidLeading || invalidTrailing)
				{
					var diagnostic = Diagnostic.Create(Rule, commentTrivia.GetLocation(), string.Empty);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}

		private static bool HasPreceedingBlankLine(SyntaxTriviaList triviaList)
		{
			var index = triviaList.Count - 1;

			var lastCommentIndex = -1;

			while (index >= 0)
			{
				switch (triviaList[index].Kind())
				{
					case SyntaxKind.SingleLineCommentTrivia:
						lastCommentIndex = index;
						break;
					case SyntaxKind.EndOfLineTrivia:

						// end of line of the previous comment
						if (index > 0 && triviaList[index - 1].IsKind(SyntaxKind.SingleLineCommentTrivia))
						{
							// jump to next comment
							lastCommentIndex = index - 1;
							index -= 2;
							continue;
						}
						else if (index < lastCommentIndex)
						{
							lastCommentIndex = -1;
						}
						break;
				}

				index--;
			}

			return lastCommentIndex == -1;
		}
	}
}
