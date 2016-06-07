using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.PlatformUI;

namespace CodeAnalysisTools.Refactorings
{
	/// <summary>
	/// Interaction logic for ExtractAssembler.xaml
	/// </summary>
	public partial class RefactoringDialog : DialogWindow
	{
		public RefactoringDialog()
		{
			InitializeComponent();
			// this.ContentTemplateSelector = new BindingPropertyTemplateSelector();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
