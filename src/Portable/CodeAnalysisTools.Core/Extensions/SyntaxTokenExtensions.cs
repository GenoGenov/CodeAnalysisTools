using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalysisTools
{
	public static class SyntaxTokenExtensions
	{
		public static bool IsPreceededByBlankLine(this SyntaxToken token)
		{
			var prevToken = token.GetPreviousToken();

			// token has > 1 EOF at the end of leading list
			if (token.LeadingTrivia.Count > 1 &&
				token.LeadingTrivia[token.LeadingTrivia.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia) &&
				token.LeadingTrivia[token.LeadingTrivia.Count - 2].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			// token has whitespace + >= 1 EOF not preceeded by comment at the end of leading list
			if (token.LeadingTrivia.Count > 2 &&
				token.LeadingTrivia[token.LeadingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) &&
				token.LeadingTrivia[token.LeadingTrivia.Count - 2].IsKind(SyntaxKind.EndOfLineTrivia) &&
				token.LeadingTrivia[token.LeadingTrivia.Count - 3].IsComment() == false)
			{
				return true;
			}

			// prev token has > 1 EOF at the end of trailing list
			if (prevToken != null && prevToken.TrailingTrivia.Count > 1 &&
				prevToken.TrailingTrivia[prevToken.TrailingTrivia.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia) &&
				prevToken.TrailingTrivia[prevToken.TrailingTrivia.Count - 2].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			// prev token has >= 1 EOF at the end of trailing list and token has >= 1 EOF at the start of leading list
			if (prevToken != null && prevToken.TrailingTrivia.Count > 0 &&
				prevToken.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia) &&
				token.LeadingTrivia.Count > 0 && token.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			return false;
		}

		public static bool IsFollowedByBlankLine(this SyntaxToken token)
		{
			var nextToken = token.GetNextToken();

			// token has > 1 EOF at the start of trailing list
			if (token.TrailingTrivia.Count > 1 &&
				token.TrailingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia) &&
				token.TrailingTrivia[1].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			// token has whitespace + >= 1 EOF at the start of trailing trivia
			if (token.TrailingTrivia.Count > 1 &&
				token.TrailingTrivia[0].IsKind(SyntaxKind.WhitespaceTrivia) &&
				token.TrailingTrivia[1].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			// next token has > 1 EOF at the start of leading list
			if (nextToken != null && nextToken.LeadingTrivia.Count > 1 &&
				nextToken.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia) &&
				nextToken.LeadingTrivia[1].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			// next token has >= 1 EOF at the start of leading list and token has >= 1 EOF at the end of trailing list
			if (nextToken != null && nextToken.LeadingTrivia.Count > 0 &&
				nextToken.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia) &&
				token.TrailingTrivia.Count > 0 && token.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
			{
				return true;
			}

			return false;
		}
	}
}
