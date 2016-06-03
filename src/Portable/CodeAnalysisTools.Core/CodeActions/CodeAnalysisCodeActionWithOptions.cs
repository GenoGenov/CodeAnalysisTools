using Microsoft.CodeAnalysis.CodeActions;

namespace CodeAnalysisTools.CodeActions
{
	public abstract class CodeAnalysisCodeActionWithOptions : CodeActionWithOptions
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
