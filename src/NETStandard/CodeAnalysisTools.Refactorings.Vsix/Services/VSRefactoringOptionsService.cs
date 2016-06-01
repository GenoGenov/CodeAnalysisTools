﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Windows;
using System.Windows.Controls;
using CodeAnalysisTools.Core.Services;
using CodeAnalysisTools.Core.ViewModel;
using CodeAnalysisTools.Refactorings.Dialogs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace CodeAnalysisTools.Refactorings.Vsix.Services
{
	[ExportLanguageService(typeof(IRefactoringOptionsService), LanguageNames.CSharp), Shared]
	public class VSRefactoringOptionsService : IRefactoringOptionsService
	{
		private readonly IDictionary<Type, Func<IRefactoringModel, UserControl>> dialogFactories;

		public VSRefactoringOptionsService()
		{
			this.dialogFactories = new Dictionary<Type, Func<IRefactoringModel, UserControl>>()
			{
				{ typeof(ExtractAssemblerViewModel), (m) => new ExtractAssemblerControl() }
			};
		}

		private RefactoringDialog dialog;

		public bool GetOptions(IRefactoringModel viewModel)
		{
			this.dialog = new RefactoringDialog();

			if (!this.dialogFactories.ContainsKey(viewModel.GetType()))
			{
				return false;
			}

			var factory = this.dialogFactories[viewModel.GetType()];
			var control = factory(viewModel);

			control.DataContext = viewModel;
			//this.dialog.Dispatcher.Invoke(
			//	() =>
			//{
			//	this.dialog.panel.Children.Add(control);
			//});

			this.dialog.DataContext = viewModel;

			return this.dialog.ShowModal().GetValueOrDefault();
		}

		public void RegisterDialogForModel<TModel>(Func<IRefactoringModel, UserControl> createFunc) where TModel : IRefactoringModel
		{
			this.dialogFactories.Add(typeof(TModel), createFunc);
		}
	}
}