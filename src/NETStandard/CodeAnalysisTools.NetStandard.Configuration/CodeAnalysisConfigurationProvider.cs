using System.IO;
using CodeAnalysisTools.Options;
using Microsoft.Extensions.Configuration;

namespace CodeAnalysisTools.Configuration
{
	public class CodeAnalysisConfigurationProvider
    {
		private const string FileConfigName = "code.analysis.json";

		public static CodeAnalysisOptions GetOptions(string basePath)
		{
			if (File.Exists(Path.Combine(basePath, FileConfigName)))
			{
				var options = new CodeAnalysisOptions();

				new ConfigurationBuilder()
					.SetBasePath(basePath)
					.AddJsonFile(FileConfigName, true, true)
					.Build()
					.Bind(options);

				return options;
			}

			return CodeAnalysisOptions.Default;
		}
	}
}
