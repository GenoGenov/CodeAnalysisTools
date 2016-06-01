using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.VisualStudio.PlatformUI;

namespace CodeAnalysisTools.Refactorings
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MainRefactoringProvider)), Shared]
	internal class ExtractAssemblerCodeRefactoringProvider : CodeRefactoringProvider
	{
		internal static ExtractAssembler modalOptions;

		private static string optionsText = string.Empty;

		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			InitWindow();

			context.RegisterRefactoring(new Refactoring(context));
		}

		private static void InitWindow()
		{
			Thread newWindowThread = new Thread(new ThreadStart(() =>
			{
				modalOptions = new ExtractAssembler();
				// When the window closes, shut down the dispatcher
				modalOptions.Closing += (_, __) =>
				 {
					 optionsText = modalOptions.docText.Text;
				 };
				modalOptions.Closed += (_, __) =>
				{
					Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
				};
				// Start the Dispatcher Processing
				System.Windows.Threading.Dispatcher.Run();
			}));

			newWindowThread.SetApartmentState(ApartmentState.STA);
			newWindowThread.IsBackground = true;
			newWindowThread.Start();
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
					return new object();
				}
				return modalOptions.Dispatcher.Invoke(() =>
				{
					if (cancellationToken.IsCancellationRequested)
					{ return (bool?)null; }
					return modalOptions.ShowModal();
				}).GetValueOrDefault();
			}

			protected override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(object options, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromResult(Enumerable.Empty<CodeActionOperation>());
				}
				return Task.FromResult(new CodeActionOperation[] { new ApplyChangesOperation(this.ctx.Document.Project.AddDocument("asd", optionsText).Project.Solution) }.AsEnumerable());
			}
		}
	}
}
