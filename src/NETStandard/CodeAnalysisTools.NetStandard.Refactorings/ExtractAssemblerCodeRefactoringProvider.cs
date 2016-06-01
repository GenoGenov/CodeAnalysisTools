using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using CodeAnalysisTools.Core.Services;
using CodeAnalysisTools.Core.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.PlatformUI;

namespace CodeAnalysisTools.Refactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MainRefactoringProvider)), Shared]
	internal class ExtractAssemblerCodeRefactoringProvider : CodeRefactoringProvider
	{
		private static string optionsText = string.Empty;

		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			context.RegisterRefactoring(new Refactoring(context));
		}

		private class Refactoring : CodeActionWithOptions
		{
			private CodeRefactoringContext ctx;

			public Refactoring(CodeRefactoringContext context)
			{
				this.ctx = context;
			}

			public override string Title
			{
				get
				{
					return "Extract Assembler...";
				}
			}

			public override object GetOptions(CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return null;
				}

				var service = this.ctx.Document.Project.LanguageServices.GetService<IRefactoringOptionsService>();

				var semantic = ctx.Document.GetSemanticModelAsync(cancellationToken).Result;
				var root = ctx.Document.GetSyntaxRootAsync().Result;

				var model = new ExtractAssemblerViewModel();
				model.ImplementDtoConvert = true;
				model.ImplementModelConvert = false;
				model.PossibleDtos = root.DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Select(x => semantic.GetDeclaredSymbol(x));
				model.PossibleModels = root.DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Select(x => semantic.GetDeclaredSymbol(x));
				model.SelectedDto = model.PossibleDtos.FirstOrDefault();
				model.SelectedModel = model.PossibleModels.FirstOrDefault();
				var result = service.GetOptions(model);

				if (result)
				{
					return model;
				}

				return null;
			}

			protected override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(object options, CancellationToken cancellationToken)
			{
				var model = options as ExtractAssemblerViewModel;

				if (cancellationToken.IsCancellationRequested || model == null)
				{
					return Task.FromResult(Enumerable.Empty<CodeActionOperation>());
				}

				return Task.FromResult(new CodeActionOperation[] { new ApplyChangesOperation(this.ctx.Document.Project.AddDocument("asd", optionsText).Project.Solution) }.AsEnumerable());
			}
		}
	}
}
