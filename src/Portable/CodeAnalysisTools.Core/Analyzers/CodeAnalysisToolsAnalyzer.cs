using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	public abstract class CodeAnalysisToolsAnalyzer : DiagnosticAnalyzer
	{
		public CodeAnalysisToolsAnalyzer()
		{
			this.Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
		}

		public abstract string DiagnosticId { get; }

		public abstract string Title { get; }

		public abstract string Description { get; }

		public abstract string MessageFormat { get; }

		public abstract string Category { get; }

		public DiagnosticDescriptor Rule { get; private set; }

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(this.Rule);
			}
		}
	}
}
