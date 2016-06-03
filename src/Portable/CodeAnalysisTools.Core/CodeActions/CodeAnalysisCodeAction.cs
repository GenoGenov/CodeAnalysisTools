using Microsoft.CodeAnalysis.CodeActions;

namespace CodeAnalysisTools.CodeActions
{
	public abstract class CodeAnalysisCodeAction : CodeAction
	{
		public sealed override string EquivalenceKey
		{
			get
			{
				return this.Title;
			}
		}
	}
}
