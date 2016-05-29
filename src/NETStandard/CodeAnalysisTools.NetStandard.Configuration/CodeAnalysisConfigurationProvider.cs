using System.ComponentModel.Composition;
using CodeAnalysisTools.Core;
using Microsoft.Extensions.Configuration;

namespace CodeAnalysisTools.Configuration
{
    public class CodeAnalysisConfigurationProvider : CodeAnalysisOptions
    {
		public static CodeAnalysisOptions options;

		IConfigurationRoot configRoot;

		public CodeAnalysisConfigurationProvider(string basePath)
		{
			this.configRoot = new ConfigurationBuilder()
				.SetBasePath(basePath)
				.AddJsonFile("code.analysis.json", false, true)
				.Build();
		}

		public override bool ImplementMethods
		{
			get
			{
				return bool.Parse(this.configRoot["implementMethods"]);
			}
		}
	}
}
