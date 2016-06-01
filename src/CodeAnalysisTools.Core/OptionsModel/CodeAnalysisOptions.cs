using CodeAnalysisTools.Core.OptionsModel;

namespace CodeAnalysisTools.Core
{
	public class CodeAnalysisOptions
	{
		public static CodeAnalysisOptions Default { get; } = new DefaultCodeAnalysisOptions();

		public ExtractDtoOptions ExtractDto { get; set; }
	}
}
