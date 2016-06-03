using CodeAnalysisTools.Models;
using Microsoft.CodeAnalysis.Host;

namespace CodeAnalysisTools.Services
{
	public interface IRefactoringOptionsService : ILanguageService
	{
		bool GetOptions(IRefactoringModel viewModel);
	}
}
