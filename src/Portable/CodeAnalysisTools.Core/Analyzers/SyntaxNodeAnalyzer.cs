using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	public abstract class SyntaxNodeAnalyzer : CodeAnalysisToolsAnalyzer
	{
		public abstract ImmutableArray<SyntaxKind> SupportedKinds { get; }

		public sealed override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(async (c) => await this.AnalyzeSyntaxNode(c), this.SupportedKinds);
		}

		public abstract Task AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context);
	}
}
