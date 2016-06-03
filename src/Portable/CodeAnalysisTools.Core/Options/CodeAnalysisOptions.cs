namespace CodeAnalysisTools.Options
{
	public class CodeAnalysisOptions
	{
		public static CodeAnalysisOptions Default { get; } = new DefaultCodeAnalysisOptions();

		public ExtractDtoOptions ExtractDto { get; set; } = new ExtractDtoOptions();

		public ExtractAssemblerOptions ExtractAssembler { get; set; } = new ExtractAssemblerOptions();
	}
}
