using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Helpers
{
	public class IndentationHelper
	{
		public static SyntaxTriviaList GetIndentationTriviaByNode(bool useTabs, int tabSize, SyntaxNode root, SyntaxToken target, CancellationToken token)
		{
			var triviaLength = root.SyntaxTree.GetLineSpan(target.Span, token).EndLinePosition.Character;

			var sourceText = root.GetText();
			var sourceLine = sourceText.Lines.GetLineFromPosition(target.SpanStart);
			var lineText = sourceText.ToString(sourceLine.Span);

			var tabsCount = lineText.ToCharArray().Count(x => x == '\t');

			triviaLength = triviaLength - (tabsCount * tabSize + tabsCount);
			if (useTabs)
			{
				return SyntaxFactory.TriviaList(Enumerable.Repeat(SyntaxFactory.Tab, 1 + triviaLength / tabSize));
			}
			else
			{
				return SyntaxFactory.TriviaList(Enumerable.Repeat<SyntaxTrivia>(SyntaxFactory.Space, triviaLength));
			}
		}

		public static SyntaxNode FormatNodeRecursive(SyntaxNode node, SyntaxTriviaList startTrivia)
		{
			return FormatNodeRecursive(node, startTrivia, 0);
		}

		private static SyntaxNode FormatNodeRecursive(SyntaxNode node, SyntaxTriviaList startTrivia, int depth)
		{
			var newTrivia = startTrivia.AddRange(Enumerable.Repeat(SyntaxFactory.Tab, depth));

			switch (node.Kind())
			{
				case SyntaxKind.Block:
					return FormatBlock(node as BlockSyntax, startTrivia);
				default:
					var newNode = node;
					if (node.HasLeadingTrivia && node.GetLeadingTrivia()[0].IsKind(SyntaxKind.EndOfLineTrivia))
					{
						newNode = newNode.WithLeadingTrivia(startTrivia);
					}
					else
					{
						var prev = node.GetFirstToken().GetPreviousToken();
						if (prev.TrailingTrivia.Count > 0 && prev.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
						{
							newNode = newNode.WithLeadingTrivia(startTrivia);
						}
					}
					newNode = newNode.ReplaceNodes(newNode.ChildNodes(), (old, potential) => FormatNodeRecursive(old, newTrivia, depth++));
					newNode = newNode
						.ReplaceTokens(newNode.ChildTokens().Where(x => x.LeadingTrivia.Count > 0 && x.LeadingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia)), (old, potential) =>
							{
								return old.WithLeadingTrivia(newTrivia);
							});

					return newNode;			  
			}
		}

		private static BlockSyntax FormatBlock(BlockSyntax block, SyntaxTriviaList startTrivia)
		{
			var statementTrivia = startTrivia.Add(SyntaxFactory.Tab);
			var prevToken = block.OpenBraceToken.GetPreviousToken();

			var newBlock = block.Update(
				block.OpenBraceToken
					.WithLeadingTrivia(startTrivia)
					.WithTrailingTrivia(block.OpenBraceToken.TrailingTrivia.AddTrailingEndOfLineIfNotExist()),
				SyntaxFactory.List(block.Statements.Select(st => IndentationHelper.FormatNodeRecursive(st, statementTrivia).WithTrailingTrivia(st.GetTrailingTrivia().AddTrailingEndOfLineIfNotExist()))),
				block.CloseBraceToken.WithLeadingTrivia(startTrivia));

			if (prevToken != null && prevToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
			{
				return newBlock = newBlock.WithLeadingTrivia(newBlock.GetLeadingTrivia().AddLeadingEndOfLineIfNotExist());
			}

			return newBlock;
		}
	}
}
