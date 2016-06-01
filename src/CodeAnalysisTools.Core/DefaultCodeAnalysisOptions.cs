using System.Collections.Generic;

namespace CodeAnalysisTools.Core
{
	public class DefaultCodeAnalysisOptions : CodeAnalysisOptions
	{
		public DefaultCodeAnalysisOptions()
		{
			this.ExtractDto = new OptionsModel.ExtractDtoOptions
			{
				ImplementMethods = false,
				Folders = new List<string>()
			};
		}
	}
}
