using System.Threading.Tasks;
using CodeAnalysisTools.Options;
using CodeAnalysisTools.Providers.CodeActions;
using CodeAnalysisTools.Refactoring;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Refactorings
{
	public class CreateUnitTestsCodeRefactoringProvider : CodeAnalysisRefactoringProvider
	{
		public async override Task ComputeRefactoringsAsync(CodeRefactoringContext context, CodeAnalysisOptions options)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);

			var decl = node as TypeDeclarationSyntax;
			if (decl != null)
			{
				context.RegisterRefactoring(CreateUnitTestsCodeAction.Create(context, decl));
			}
		}
	}
}
