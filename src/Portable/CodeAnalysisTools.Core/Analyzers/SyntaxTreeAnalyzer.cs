using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	public abstract class SyntaxTreeAnalyzer : CodeAnalysisToolsAnalyzer
	{
		public sealed override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxTreeAction(async (c) => await this.AnalyzeSyntaxTree(c));
		}

		public abstract Task AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context);
	}
}
