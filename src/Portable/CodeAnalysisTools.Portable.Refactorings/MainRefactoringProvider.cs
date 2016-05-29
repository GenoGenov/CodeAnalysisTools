using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using CodeAnalysisTools.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace CodeAnalysisTools.Refactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MainRefactoringProvider)), Shared]
	public class MainRefactoringProvider : CodeRefactoringProvider
	{
		private static ImmutableArray<CodeAnalysisRefactoringProvider> RefactoringProviders = ImmutableArray.Create<CodeAnalysisRefactoringProvider>(
			new ExtractDtoCodeRefactoringProvider()
			);

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			foreach (var provider in RefactoringProviders)
			{
				await provider.ComputeRefactoringsAsync(context, CodeAnalysisOptions.Default);
			}
		}
	}
}
