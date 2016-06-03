using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisTools.Models
{
	public class ExtractConverterModel : IRefactoringModel
	{
		public IEnumerable<ISymbol> PossibleDtos { get; set; }

		public IEnumerable<ISymbol> PossibleModels { get; set; }

		public ISymbol SelectedDto { get; set; }

		public ISymbol SelectedModel { get; set; }

		public bool ImplementModelConvert { get; set; }

		public bool ImplementModelConvertDisabled { get; set; }

		public bool ImplementDtoConvert { get; set; }

		public bool ImplementDtoConvertDisabled { get; set; }

		public bool EnableDtoSelect { get; set; }

		public bool EnableModelSelect { get; set; }
	}
}
