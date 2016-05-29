namespace CodeAnalysisTools.Core
{
	public class CodeAnalysisOptions
	{
		public static CodeAnalysisOptions Default { get; } = new DefaultCodeAnalysisOptions();

		public virtual bool ImplementMethods { get; }
	}
}
