using System;
using CodeAnalysisTools.Refactorings.Vsix.UI;

namespace CodeAnalysisTools.Refactorings.Vsix
{
	public class BindingProperty : IBindingProperty
	{
		public Type PropertyType { get; set; }

		public string Name { get; set; }

		public object Value { get; set; }

		public bool IsSelected { get; set; }
	}
}