using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NamespaceNameAnalyzer : CompilationStartAnalyzer
	{
		public override string Category
		{
			get
			{
				return "Readability";
			}
		}

		public override string Description
		{
			get
			{
				return "Namespace name must follow assembly directories structure.";
			}
		}

		public override string DiagnosticId
		{
			get
			{
				return "CATA008";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "Namespace name must follow assembly directories structure.";
			}
		}

		public override string Title
		{
			get
			{
				return "Namespace name must follow assembly directories structure.";
			}
		}

		public async override Task AnalyzeCompilationStart(CompilationStartAnalysisContext context)
		{

			context.RegisterSyntaxTreeAction(
					(syntaxTreeContext) =>
						{
							var semModel = context.Compilation.GetSemanticModel(syntaxTreeContext.Tree);
							var filePath = syntaxTreeContext.Tree.FilePath;
							if (filePath == null)
							{
								return;
							}

							var parentDirectory = System.IO.Path.GetDirectoryName(filePath);
							var parentDirectoryWithDots = parentDirectory.Replace("\\", ".");
							var namespaceNodes = syntaxTreeContext.Tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>();
							foreach (var ns in namespaceNodes)
							{
								var symbolInfo = semModel.GetDeclaredSymbol(ns) as INamespaceSymbol;
								var name = symbolInfo.ToDisplayString();
								if (!NameEndsWithNamespace(parentDirectoryWithDots, name))
								{
									syntaxTreeContext.ReportDiagnostic(
										Diagnostic.Create(Rule, ns.Name.GetLocation(), parentDirectoryWithDots));
								}
							}
						});
		}

		private bool NameEndsWithNamespace(string name, string namespaceName)
		{
			var nameParts = name.Split('.').Reverse().ToArray();
			var namespaceParts = namespaceName.Split('.').Reverse().ToArray();

			if (nameParts.Length < namespaceParts.Length)
			{
				return false;
			}

			for (int i = 0; i < namespaceParts.Length; i++)
			{
				if (namespaceParts[i] != nameParts[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}