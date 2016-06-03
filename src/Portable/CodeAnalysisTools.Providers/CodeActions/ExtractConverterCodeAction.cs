using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.CodeActions;
using CodeAnalysisTools.Models;
using CodeAnalysisTools.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Providers.CodeActions
{
	public class ExtractConverterCodeAction : CodeAnalysisCodeActionWithOptions
	{
		private readonly Document document;
		private readonly CancellationToken token;
		private readonly TypeDeclarationSyntax declaration;

		public static CodeActionWithOptions Create(CodeRefactoringContext context, TypeDeclarationSyntax declaration)
		{
			return new ExtractConverterCodeAction(context.Document, context.CancellationToken, declaration);
		}

		public ExtractConverterCodeAction(Document document, CancellationToken token, TypeDeclarationSyntax declaration)
		{
			this.document = document;
			this.token = token;
			this.declaration = declaration;
		}

		public override string Title
		{
			get
			{
				return "Extract Assembler...";
			}
		}

		public override object GetOptions(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return null;
			}

			var service = document.Project.LanguageServices.GetService<IRefactoringOptionsService>();

			var model = new ExtractConverterModel();

			var semantics = this.document.GetSemanticModelAsync(this.token).Result;
			var symbol = semantics.GetDeclaredSymbol(this.declaration);
			var symbolText = symbol.Name.EndsWith("dto", StringComparison.OrdinalIgnoreCase) ? symbol.Name.Substring(0, symbol.Name.Length - 3) : symbol.Name;

			bool isInterface = this.declaration.IsKind(SyntaxKind.InterfaceDeclaration);
			if (this.declaration.Identifier.Text.EndsWith("dto", StringComparison.OrdinalIgnoreCase) && isInterface == false)
			{
				model.EnableDtoSelect = false;
				model.EnableModelSelect = true;
				model.SelectedDto = symbol;
				model.PossibleModels = this.LookupPossibleTypes(
				symbol,
				x => x.Name.ToUpper().Contains(symbolText.ToUpper())
				&&
				x.Name.EndsWith("dto", StringComparison.OrdinalIgnoreCase) == false
				&&
				x.Name.EndsWith("provider", StringComparison.OrdinalIgnoreCase) == false
				&&
				x.Name.EndsWith("service", StringComparison.OrdinalIgnoreCase) == false
				&&
				x.Name.EndsWith("converter", StringComparison.OrdinalIgnoreCase) == false
				&&
				x.Name.EndsWith("controller", StringComparison.OrdinalIgnoreCase) == false
				&&
				x.Name.EndsWith("builder", StringComparison.OrdinalIgnoreCase) == false)
				.OrderByDescending(x => x.Name.EndsWith("Model")).ThenByDescending(x => x.Name.StartsWith("I"));

				model.SelectedModel = model.PossibleModels.FirstOrDefault();
			}
			else
			{
				model.EnableDtoSelect = true;
				model.EnableModelSelect = false;
				model.PossibleDtos = this.LookupPossibleTypes(
					symbol,
					x => x.Name.ToUpper().Contains(symbolText.ToUpper())
					&&
					x.Name.EndsWith("dto", StringComparison.OrdinalIgnoreCase));
				model.SelectedModel = symbol;

				model.SelectedDto = model.PossibleDtos.FirstOrDefault();
			}

			if (isInterface)
			{
				model.ImplementDtoConvertDisabled = true;
				model.ImplementDtoConvert = false;
			}

			var result = service.GetOptions(model);

			if (result)
			{
				return model;
			}

			return null;
		}

		protected async override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(object options, CancellationToken cancellationToken)
		{
			var model = options as ExtractConverterModel;

			if (cancellationToken.IsCancellationRequested || model == null)
			{
				return Enumerable.Empty<CodeActionOperation>();
			}

			var solution = await this.ImplementConverterAsync(cancellationToken, model);

			return new CodeActionOperation[] { new ApplyChangesOperation(solution) };
		}

		private IEnumerable<ISymbol> LookupPossibleTypes(INamedTypeSymbol symbol, Func<INamedTypeSymbol, bool> predicate)
		{
			var symbols = new List<ISymbol>();

			var allLinked = this.GetAllLinkedProjects(this.document.Project);

			symbols.AddRange(
				allLinked.Select(x => x.GetCompilationAsync().Result)
				.SelectMany(x => x.SyntaxTrees.Select(s => x.GetSemanticModel(s)))
				.SelectMany(
					semanticModel =>
					semanticModel.SyntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
					.Select(decl => semanticModel.GetDeclaredSymbol(decl)))
				.Where(predicate)
				.ToList());

			return symbols;
		}

		private Task<Solution> ImplementConverterAsync(CancellationToken cancellationToken, ExtractConverterModel model)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns all projects that either have a reference to or are referenced by the provided project in a solution.
		/// </summary>
		/// <param name="project"></param>
		/// <returns></returns>
		private IEnumerable<Project> GetAllLinkedProjects(Project project)
		{
			var sln = project.Solution;

			var references = project.ProjectReferences;

			var result = new List<Project>() { project };

			foreach (var proj in sln.Projects)
			{
				if (references.Any(x => x.ProjectId == proj.Id) || proj.ProjectReferences.Any(x => x.ProjectId == project.Id))
				{
					result.Add(proj);
				}
			}

			return result;
		}
	}
}
