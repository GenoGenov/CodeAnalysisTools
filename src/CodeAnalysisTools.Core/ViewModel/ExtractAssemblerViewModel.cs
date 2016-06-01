using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisTools.Core.ViewModel
{
	public class ExtractAssemblerViewModel : IRefactoringModel
	{
		public IEnumerable<ISymbol> PossibleDtos { get; set; }

		public IEnumerable<ISymbol> PossibleModels { get; set; }

		public ISymbol SelectedDto { get; set; }

		public ISymbol SelectedModel { get; set; }

		public bool ImplementModelConvert { get; set; }

		public bool ImplementDtoConvert { get; set; }
	}
}
