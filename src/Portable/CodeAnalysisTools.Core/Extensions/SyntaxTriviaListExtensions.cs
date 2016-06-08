using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalysisTools
{
	public static class SyntaxTriviaListExtensions
	{
		public static SyntaxTriviaList AddTrailingEndOfLineIfNotExist(this SyntaxTriviaList trivia)
		{
			if (trivia.Any(SyntaxKind.EndOfLineTrivia) == false)
			{
				return trivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));
			}

			return trivia;
		}

		public static SyntaxTriviaList AddLeadingEndOfLineIfNotExist(this SyntaxTriviaList trivia)
		{
			if (trivia.Any(SyntaxKind.EndOfLineTrivia) == false)
			{
				return trivia.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine));
			}

			return trivia;
		}

		public static bool HasConsecutiveEndLineTrivia(this SyntaxTriviaList trivia)
		{
			var list = trivia.ToList();
			if (list.Count > 1)
			{
				for (int i = 0; i < list.Count - 1; i++)
				{
					if (list[i].IsKind(SyntaxKind.SingleLineCommentTrivia) || list[i].IsKind(SyntaxKind.MultiLineCommentTrivia))
					{
						i++;
						continue;
					}
					if (list[i].IsKind(SyntaxKind.EndOfLineTrivia) && list[i + 1].IsKind(SyntaxKind.EndOfLineTrivia))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool HasVisibleEndOfLineTrivia(this SyntaxTriviaList trivia)
		{
			var list = trivia.ToList();
			if (list.Count > 1)
			{
				for (int i = 0; i < list.Count - 1; i++)
				{
					if (list[i].IsKind(SyntaxKind.SingleLineCommentTrivia) || list[i].IsKind(SyntaxKind.MultiLineCommentTrivia))
					{
						i++;
						continue;
					}
					if (list[i].IsKind(SyntaxKind.EndOfLineTrivia))
					{
						return true;
					}
				}
			}
			else if (list.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
			{
				return true;
			}

			return false;
		}

		public static int IndexOf(this SyntaxTriviaList trivia, SyntaxKind triviaKind, int startIndex)
		{
			for (int i = startIndex; i < trivia.Count; i++)
			{
				if (trivia[i].IsKind(triviaKind))
				{
					return i;
				}
			}

			return -1;
		}

		public static List<SyntaxTriviaList> SplitBy(this SyntaxTriviaList list, SyntaxKind triviaKind, bool keepSplitTrivia = false)
		{
			var result = new List<SyntaxTriviaList>();

			if (list.Count < 2)
			{
				result.Add(list);
				return result;
			}

			IEnumerable<SyntaxTrivia> currentList = null;

			var currentCount = 0;

			while (currentCount < list.Count)
			{
				currentList = list.Skip(currentCount).TakeWhile(x => x.IsKind(triviaKind) == false);

				var triviaList = currentList.ToSyntaxTriviaList();

				currentCount += triviaList.Count + 1;

				if (keepSplitTrivia && currentCount < list.Count)
				{
					triviaList = triviaList.Add(list[currentCount - 1]);
				}

				if (currentList.Any())
				{
					result.Add(triviaList);
				}
			}

			return result;
		}

		public static SyntaxTriviaList RemoveConsecutiveEndLines(this SyntaxTriviaList trivia)
		{
			var result = SyntaxFactory.TriviaList();

			var list = trivia.ToList();
			if (list.Count > 1)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (i < list.Count - 1 && list[i].IsKind(SyntaxKind.EndOfLineTrivia) && list[i + 1].IsKind(SyntaxKind.EndOfLineTrivia))
					{
						continue;
					}

					result = result.Add(list[i]);
				}
			}
			else
			{
				result = result.AddRange(list);
			}

			return result;
		}

		public static bool IsComment(this SyntaxTrivia trivia)
		{
			return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
		}
	}
}
