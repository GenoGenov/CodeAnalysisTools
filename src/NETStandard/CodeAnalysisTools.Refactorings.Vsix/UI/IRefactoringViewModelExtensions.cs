using System;
using System.Collections.Generic;
using System.Reflection;
using CodeAnalysisTools.Core.ViewModel;
using CodeAnalysisTools.Refactorings.Vsix;
using CodeAnalysisTools.Refactorings.Vsix.UI;

namespace CodeAnalysisTools.Refactorings
{
	internal static class IRefactoringViewModelExtensions
	{
		public static IEnumerable<IBindingProperty> ToBindingPropertyEnumerable(this IRefactoringModel model)
		{
			var result = new List<BindingProperty>();

			var props = model.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

			foreach (var prop in props)
			{
				result.Add(
				new BindingProperty
				{
					Name = prop.Name,
					PropertyType = prop.PropertyType,
					Value = prop.GetValue(model)
				});
			}

			return result;
		}

		public static bool IsType(this IBindingProperty property, Type type)
		{
			return property.PropertyType == type;
		}

		public static bool IsType<TType>(this IBindingProperty property)
		{
			return property.IsType(typeof(TType));
		}
	}
}
