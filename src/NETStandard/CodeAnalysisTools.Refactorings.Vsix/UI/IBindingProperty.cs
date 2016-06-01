using System;

namespace CodeAnalysisTools.Refactorings.Vsix.UI
{
	internal interface IBindingProperty
	{
		Type PropertyType { get; }

		string Name { get; }

		object Value { get; }

		bool IsSelected { get; }
	}
}
