using System.Threading.Tasks;
using CodeAnalysisTools.Options;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace CodeAnalysisTools.Refactoring
{
	public abstract class CodeAnalysisRefactoringProvider
	{
		public abstract Task ComputeRefactoringsAsync(CodeRefactoringContext context, CodeAnalysisOptions options);
	}
}
