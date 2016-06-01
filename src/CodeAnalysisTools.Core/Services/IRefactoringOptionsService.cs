using CodeAnalysisTools.Core.ViewModel;
using Microsoft.CodeAnalysis.Host;

namespace CodeAnalysisTools.Core.Services
{
	public interface IRefactoringOptionsService : ILanguageService
	{
		bool GetOptions(IRefactoringModel viewModel);
	}
}
