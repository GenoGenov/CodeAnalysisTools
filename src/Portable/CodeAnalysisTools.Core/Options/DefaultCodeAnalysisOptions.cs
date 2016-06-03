using System.Collections.Generic;

namespace CodeAnalysisTools.Options
{
	public class DefaultCodeAnalysisOptions : CodeAnalysisOptions
	{
		public DefaultCodeAnalysisOptions()
		{
			this.ExtractDto = new ExtractDtoOptions
			{
				ImplementMethods = false,
				Folders = new List<string>()
			};
		}
	}
}
