using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools
{
	public static class SyntaxTokenExtensions
	{
		public static bool IsPreceededByBlankLine(this SyntaxToken token)
		{
			var prevToken = token.GetPreviousToken();

			var allPreceedingTrivia = token.LeadingTrivia;

			if (prevToken != null)
			{
				allPreceedingTrivia = prevToken.TrailingTrivia.AddRange(allPreceedingTrivia);
			}

			int eofCount = 0;

			foreach (var trivia in allPreceedingTrivia.Reverse())
			{
				var kind = trivia.Kind();

				if (kind == SyntaxKind.WhitespaceTrivia)
				{
					continue;
				}
				else if (kind == SyntaxKind.EndOfLineTrivia)
				{
					eofCount++;
					continue;
				}
				//else if (trivia.IsComment())
				//{
				//	eofCount--;
				//	break;
				//}
				else
				{
					break;
				}
			}

			return eofCount > 1;
		}

		public static bool IsFollowedByBlankLine(this SyntaxToken token)
		{
			var nextToken = token.GetNextToken();

			var allTrailingTrivia = token.TrailingTrivia;

			if (nextToken != null)
			{
				allTrailingTrivia = allTrailingTrivia.AddRange(nextToken.LeadingTrivia);
			}

			int eofCount = 0;

			foreach (var trivia in allTrailingTrivia)
			{
				var kind = trivia.Kind();

				if (kind == SyntaxKind.WhitespaceTrivia)
				{
					continue;
				}
				else if (kind == SyntaxKind.EndOfLineTrivia)
				{
					eofCount++;
					continue;
				}
				else
				{
					break;
				}
			}

			return eofCount > 1;
		}

		public static SyntaxNode RemovePreceedingBlankLine(this SyntaxToken token)
		{
			int eofCount = 0;

			bool addAllToEnd = false;

			var node = token.Parent;

			var prevToken = token.GetPreviousToken();

			var allPreceedingTrivia = token.LeadingTrivia;

			if (prevToken != null)
			{
				allPreceedingTrivia = prevToken.TrailingTrivia.AddRange(allPreceedingTrivia);
			}

			var newTrailingTrivia = new List<SyntaxTrivia>();

			foreach (var trivia in allPreceedingTrivia.Reverse())
			{
				var kind = trivia.Kind();

				if (addAllToEnd)
				{
					newTrailingTrivia.Add(trivia);
					continue;
				}

				if (kind == SyntaxKind.WhitespaceTrivia)
				{
					newTrailingTrivia.Add(trivia);
				}
				else if (eofCount == 0 && kind == SyntaxKind.EndOfLineTrivia)
				{
					eofCount++;

					continue;
				}
				else
				{
					newTrailingTrivia.Add(trivia);
					addAllToEnd = true;
				}
			}

			newTrailingTrivia.Reverse();

			var result = node.ReplaceTokens(new[] { prevToken, token}, (old, potential)=> 
			{
				if (old == prevToken)
				{
					return prevToken.WithTrailingTrivia(newTrailingTrivia);
				}
				else
				{
					return token.WithLeadingTrivia(SyntaxFactory.TriviaList());
				}
			});

			return result;
		}

		public static SyntaxNode RemoveFollowingBlankLine(this SyntaxToken token)
		{
			int eofCount = 0;

			bool addAllToEnd = false;

			var node = token.Parent;

			var nextToken = token.GetNextToken();

			var allFollowingTrivia = token.TrailingTrivia;

			if (nextToken != null)
			{
				allFollowingTrivia = allFollowingTrivia.AddRange(nextToken.LeadingTrivia);
			}

			var newTrailingTrivia = new List<SyntaxTrivia>();

			foreach (var trivia in allFollowingTrivia)
			{
				var kind = trivia.Kind();

				if (addAllToEnd)
				{
					newTrailingTrivia.Add(trivia);
					continue;
				}

				if (kind == SyntaxKind.WhitespaceTrivia)
				{
					newTrailingTrivia.Add(trivia);
				}
				else if (eofCount == 0 && kind == SyntaxKind.EndOfLineTrivia)
				{
					eofCount++;

					continue;
				}
				else
				{
					newTrailingTrivia.Add(trivia);
					addAllToEnd = true;
				}
			}

			var result = node.ReplaceTokens(new[] { nextToken, token }, (old, potential) =>
			{
				if (old == nextToken)
				{
					return nextToken.WithLeadingTrivia(SyntaxFactory.TriviaList());
				}
				else
				{
					return token.WithTrailingTrivia(newTrailingTrivia);
				}
			});

			return result;
		}
	}
}
