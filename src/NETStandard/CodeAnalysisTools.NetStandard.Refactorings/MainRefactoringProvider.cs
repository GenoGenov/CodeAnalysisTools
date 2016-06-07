using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using CodeAnalysisTools.Configuration;
using CodeAnalysisTools.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace CodeAnalysisTools.Refactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MainRefactoringProvider)), Shared]
	public class MainRefactoringProvider : CodeRefactoringProvider
	{
		private static ImmutableArray<CodeAnalysisRefactoringProvider> RefactoringProviders = ImmutableArray.Create<CodeAnalysisRefactoringProvider>(
			new ExtractDtoCodeRefactoringProvider(),
			// new ExtractConverterCodeRefactoringProvider(),
			new CreateUnitTestsCodeRefactoringProvider()
			);

		public sealed async override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var optionsProvider = CodeAnalysisConfigurationProvider.Create(Path.Combine(context.Document.Project.Solution.FilePath, "..\\"));
			var options = optionsProvider.GetOptions();

			foreach (var provider in RefactoringProviders)
			{
				await provider.ComputeRefactoringsAsync(context, options);
			}
		}
	}
}
