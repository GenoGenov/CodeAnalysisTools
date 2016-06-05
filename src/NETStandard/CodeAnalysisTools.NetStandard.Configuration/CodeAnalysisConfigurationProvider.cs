using System.IO;
using System.Text;
using CodeAnalysisTools.NetStandard.Configuration;
using CodeAnalysisTools.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CodeAnalysisTools.Configuration
{
	public class CodeAnalysisConfigurationProvider
	{
		private static object syncLock = new object();
		private static CodeAnalysisConfigurationProvider instance;
		private const string FileConfigName = "code.analysis.json";

		private readonly IConfigurationRoot root;
		private readonly string basePath;

		private CodeAnalysisConfigurationProvider()
		{
		}

		private CodeAnalysisConfigurationProvider(string basePath) : this()
		{
			this.basePath = basePath;

						if (File.Exists(Path.Combine(basePath, FileConfigName)) == false)
			{
				using (var optionsFile = File.Open(Path.Combine(basePath, FileConfigName), FileMode.OpenOrCreate, FileAccess.Write))
				{
					var options = CodeAnalysisOptions.Default;
					var bytes = Encoding.UTF8.GetBytes(FileConfigurationOptions.GetDefaultJson(options));
					optionsFile.Write(bytes, 0, bytes.Length);
					optionsFile.Flush();
				}
			}

			this.root = new ConfigurationBuilder()
							.SetBasePath(basePath)
							.AddJsonFile(FileConfigName, true, true)
							.Build();
		}

		public static CodeAnalysisConfigurationProvider Create(string basePath)
		{
			if (instance == null)
			{
				lock (syncLock)
				{
					if (instance == null)
					{
						instance = new CodeAnalysisConfigurationProvider(basePath);
					}
				}
			}

			return instance;
		}

		public CodeAnalysisOptions GetOptions()
		{
			return FileConfigurationOptions.FromRoot(this.root);
		}

		public void SaveOptions(CodeAnalysisOptions options)
		{
			using (var optionsFile = File.Open(Path.Combine(this.basePath, FileConfigName), FileMode.OpenOrCreate, FileAccess.Write))
			{
				var bytes = Encoding.UTF8.GetBytes(FileConfigurationOptions.GetDefaultJson(options));
				optionsFile.Write(bytes, 0, bytes.Length);
				optionsFile.Flush();
			}
		}
	}
}
