using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace CodeAnalysisTools.Core
{
	public abstract class CodeAnalysisRefactoringProvider
	{
		public abstract Task ComputeRefactoringsAsync(CodeRefactoringContext context, CodeAnalysisOptions options);
	}
}
