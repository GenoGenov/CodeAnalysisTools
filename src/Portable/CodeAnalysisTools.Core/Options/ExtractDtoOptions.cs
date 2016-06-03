using System.Collections.Generic;

namespace CodeAnalysisTools.Options
{
	public class ExtractDtoOptions
	{
		public string Project { get; set; }

		public List<string> Folders { get; set; }

		public virtual bool ImplementMethods { get; set; }

		public string DefaultNamespace { get; set; }
	}
}
