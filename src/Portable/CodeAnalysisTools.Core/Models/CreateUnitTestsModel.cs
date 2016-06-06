using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisTools.Models
{
	public class CreateUnitTestsModel : IRefactoringModel
	{
		public IEnumerable<string> PossibleAssemblies { get; set; }

		public string SelectedAssembly { get; set; }

		public bool CreaneNewAssembly { get; set; }

		public string NewAssemblyName { get; set; }
	}
}
