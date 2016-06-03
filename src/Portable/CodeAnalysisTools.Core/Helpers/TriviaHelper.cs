using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeAnalysisTools.Helpers
{
	public static class TriviaHelper
	{
		public static SyntaxTriviaList FixSingleLineCommentSpacing(SyntaxTriviaList list)
		{
			var parts = list.SplitBy(SyntaxKind.SingleLineCommentTrivia, true);
			if (parts.Count == 1)
			{
				return parts[0];
			}

			var result = SyntaxFactory.TriviaList();

			return result
				.AddRange(parts[0].AddLeadingEndOfLineIfNotExist())
				.AddRange(parts.Skip(1).SelectMany(x => x.RemoveConsecutiveEndLines()).ToList());
		}
	}
}
