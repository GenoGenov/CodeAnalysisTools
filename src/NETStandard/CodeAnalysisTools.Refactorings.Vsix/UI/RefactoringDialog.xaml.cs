using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeAnalysisTools.Refactorings.Vsix.UI;
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
	}

	public class BindingPropertyTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var prop = item as IBindingProperty;
			var element = container as FrameworkElement;

			if (prop.IsType<ISymbol>())
			{
				return (DataTemplate)element.Resources["symbol"];
			}
			if (prop.IsType<IEnumerable<ISymbol>>())
			{
				return (DataTemplate)element.Resources["symbolarr"];
			}
			if (prop.IsType<bool>())
			{
				return (DataTemplate)element.Resources["bool"];
			}

			return base.SelectTemplate(item, container);
		}
	}
}
