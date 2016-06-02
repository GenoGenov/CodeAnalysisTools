using System.Threading.Tasks;
using CodeAnalysisTools.Core;
using CodeAnalysisTools.Providers.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Refactorings
{
	public class ExtractConverterCodeRefactoringProvider : CodeAnalysisRefactoringProvider
	{
		private const string BaseConverterTypeName = "DtoConverterBase";

		public async override Task ComputeRefactoringsAsync(CodeRefactoringContext context, CodeAnalysisOptions options)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);

			var decl = node as TypeDeclarationSyntax;
			if (decl != null)
			{
				context.RegisterRefactoring(ExtractConverterCodeAction.Create(context, decl));
			}
		}
	}
}
