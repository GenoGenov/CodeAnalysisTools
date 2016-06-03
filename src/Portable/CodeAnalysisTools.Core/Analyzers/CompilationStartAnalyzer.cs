using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	public abstract class CompilationStartAnalyzer : CodeAnalysisToolsAnalyzer
	{
		public sealed override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction(async (c) => await this.AnalyzeCompilationStart(c));
		}

		public abstract Task AnalyzeCompilationStart(CompilationStartAnalysisContext context);
	}
}
