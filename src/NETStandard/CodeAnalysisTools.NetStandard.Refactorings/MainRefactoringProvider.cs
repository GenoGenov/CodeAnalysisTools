using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using CodeAnalysisTools.Configuration;
using CodeAnalysisTools.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodeAnalysisTools.Refactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MainRefactoringProvider)), Shared]
	public class MainRefactoringProvider : CodeRefactoringProvider
	{
		private static ImmutableArray<CodeAnalysisRefactoringProvider> RefactoringProviders = ImmutableArray.Create<CodeAnalysisRefactoringProvider>(
			new ExtractDtoCodeRefactoringProvider()
			);

		public sealed async override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var options = CodeAnalysisConfigurationProvider.GetOptions(Path.Combine(context.Document.Project.Solution.FilePath, "..\\"));

			foreach (var provider in RefactoringProviders)
			{
				await provider.ComputeRefactoringsAsync(context, options);
			}
		}
	}
}
