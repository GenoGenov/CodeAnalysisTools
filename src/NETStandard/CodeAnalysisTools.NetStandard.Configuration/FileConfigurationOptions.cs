using System.Collections.Generic;
using System.Linq;
using CodeAnalysisTools.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CodeAnalysisTools.NetStandard.Configuration
{
	internal class FileConfigurationOptions : CodeAnalysisOptions
	{
		public static CodeAnalysisOptions FromRoot(IConfigurationRoot provider)
		{
			var options = new CodeAnalysisOptions
			{
				ExtractAssembler = new ExtractAssemblerOptions
				{
					ImplementDtoConvert = provider.GetValue("ExtractAssembler:ImplementDtoConvert", CodeAnalysisOptions.Default.ExtractAssembler.ImplementDtoConvert),
					ImplementModelConvert = provider.GetValue("ExtractAssembler:ImplementModelConvert", CodeAnalysisOptions.Default.ExtractAssembler.ImplementModelConvert)
				},
				ExtractDto = new ExtractDtoOptions
				{
					DefaultNamespace = provider.GetValue("ExtractDto:DefaultNamespace", CodeAnalysisOptions.Default.ExtractDto.DefaultNamespace),
					Folders = provider.GetValue("ExtractDto:Folders", CodeAnalysisOptions.Default.ExtractDto.Folders),
					ImplementMethods = provider.GetValue("ExtractDto:ImplementMethods", CodeAnalysisOptions.Default.ExtractDto.ImplementMethods),
					Project = provider.GetValue("ExtractDto:Project", CodeAnalysisOptions.Default.ExtractDto.Project)
				}
			};

			return options;
		}

		public static string GetDefaultJson(CodeAnalysisOptions options)
		{
			return JsonConvert.SerializeObject(options, Formatting.Indented);
		}
	}
}
