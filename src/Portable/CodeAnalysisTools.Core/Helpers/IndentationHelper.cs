using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Helpers
{
	public class IndentationHelper
	{
		public static SyntaxTriviaList GetIndentationTriviaByNode(SyntaxNode root, SyntaxToken target, CancellationToken token)
		{
			var triviaLength = root.SyntaxTree.GetLineSpan(target.Span, token).StartLinePosition.Character;

			var sourceText = root.GetText();
			var sourceLine = sourceText.Lines.GetLineFromPosition(target.SpanStart);
			var lineText = sourceText.ToString(sourceLine.Span);
			var startTabs = lineText.ToCharArray().TakeWhile(x => x == '\t').Count();

			var tabs = (triviaLength - startTabs) / 4 + startTabs;

			var tabsLeft = (triviaLength - startTabs) % 4;

			//if (tabsLeft > 0)
			//{
			//	tabs++;
			//}

			var result = SyntaxFactory.TriviaList(Enumerable.Repeat(SyntaxFactory.Tab, tabs));

			result = result.AddRange(Enumerable.Repeat(SyntaxFactory.Space, tabsLeft));

			return result;
		}

		public static BlockSyntax FormatBlockRecursive(BlockSyntax block, SyntaxTriviaList startTrivia)
		{
			var result = FormatBlock(block, startTrivia);
			var blocks = result.Statements.SelectMany(s => s.DescendantNodes(x => x.IsKind(SyntaxKind.Block) == false).OfType<BlockSyntax>());
			if (blocks.Any())
			{
				var statementTrivia = startTrivia.Add(SyntaxFactory.Tab);
				foreach (var innerBlock in blocks)
				{
					var newBlock =
					result = result.ReplaceNode(innerBlock, FormatBlockRecursive(innerBlock, statementTrivia));
				}
			}

			return result;
		}

		private static BlockSyntax FormatBlock(BlockSyntax block, SyntaxTriviaList startTrivia)
		{
			var statementTrivia = startTrivia.Add(SyntaxFactory.Tab);
			var prevToken = block.OpenBraceToken.GetPreviousToken();

			var newBlock = block.Update(
				block.OpenBraceToken
					.WithLeadingTrivia(startTrivia)
					.WithTrailingTrivia(block.OpenBraceToken.TrailingTrivia.AddTrailingEndOfLineIfNotExist()),
				SyntaxFactory.List(block.Statements.Select(st => st.WithLeadingTrivia(statementTrivia).WithTrailingTrivia(st.GetTrailingTrivia().AddTrailingEndOfLineIfNotExist()))),
				block.CloseBraceToken.WithLeadingTrivia(startTrivia));

			if (prevToken != null && prevToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
			{
				return newBlock = newBlock.WithLeadingTrivia(newBlock.GetLeadingTrivia().AddLeadingEndOfLineIfNotExist());
			}

			return newBlock;
		}
	}
}
