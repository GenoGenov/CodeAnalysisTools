using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CodeAnalysisTools.Refactorings.Dialogs
{
	public partial class CreateUnitTestsControl : UserControl
	{
		public CreateUnitTestsControl()
		{
			InitializeComponent();
		}
	}

	public class InverseBooleanToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
				throw new InvalidOperationException("The target must be a boolean");

			if (!(bool)value)
			{
				return Visibility.Visible;
			}
			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}

	public class InvertBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool booleanValue = (bool)value;
			return !booleanValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool booleanValue = (bool)value;
			return !booleanValue;
		}
	}
}
