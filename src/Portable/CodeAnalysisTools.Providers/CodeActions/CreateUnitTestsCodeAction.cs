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
	public class CreateUnitTestsCodeAction : CodeAnalysisCodeActionWithOptions
	{
		private readonly Document document;
		private readonly TypeDeclarationSyntax declaration;

		private INamedTypeSymbol selectedType;

		public CreateUnitTestsCodeAction(Document document, TypeDeclarationSyntax declaration)
		{
			this.document = document;
			this.declaration = declaration;
		}

		public static CodeActionWithOptions Create(CodeRefactoringContext context, TypeDeclarationSyntax declaration)
		{
			return new CreateUnitTestsCodeAction(context.Document, declaration);
		}

		public override string Title
		{
			get
			{
				return "Create unit tests..";
			}
		}

		public override object GetOptions(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return null;
			}

			var service = document.Project.LanguageServices.GetService<IRefactoringOptionsService>();
			var model = new CreateUnitTestsModel();

			var semantics = this.document.GetSemanticModelAsync(cancellationToken).Result;
			this.selectedType = semantics.GetDeclaredSymbol(this.declaration);

			var currentAssembly = this.document.Project.AssemblyName;
			var newAssembly = currentAssembly + ".UnitTests";

			model.PossibleAssemblies = this.document.Project.Solution.Projects.Where(x => x.Name.ToUpper().Contains("test".ToUpper())).Select(x => x.AssemblyName);
			model.SelectedAssembly = model.PossibleAssemblies.FirstOrDefault(x => x.ToUpper().Contains(currentAssembly.ToUpper()));
			
			var result = service.GetOptions(model);
			if (result)
			{
				return model;
			}
			return null;
		}

		protected async override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(object options, CancellationToken cancellationToken)
		{
			var model = options as CreateUnitTestsModel;

			Project project = null;

			if (model.CreaneNewAssembly)
			{
				project = this.document.Project.Solution.AddProject(model.NewAssemblyName, model.NewAssemblyName, LanguageNames.CSharp);
			}
			else
			{
				project = this.document.Project.Solution.Projects.First(x => x.AssemblyName == model.SelectedAssembly);
			}


			project = await this.CreateDocuments(project);

			return new[] { new ApplyChangesOperation(project.Solution) };
		}

		private async Task<Project> CreateDocuments(Project project)
		{
			var syntaxTree = await this.document.GetSyntaxTreeAsync();

			var methods = this.declaration.Members.OfType<MethodDeclarationSyntax>().Where(x => x.Modifiers.Any(SyntaxKind.PublicKeyword));

			foreach (var method in methods)
			{
				var newNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(project.AssemblyName + "." + this.declaration.Identifier.Text + "Tests"));

				var classDecl = SyntaxFactory.ClassDeclaration(
					SyntaxFactory.Identifier(method.Identifier.Text + "_Should")
					.WithAdditionalAnnotations(RenameAnnotation.Create()))
					.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
					.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestClass")) })));

				var finalRoot = newNamespace.AddMembers(classDecl);

				project = project.AddDocument(method.Identifier.Text + "_Should", finalRoot, new[] { this.declaration.Identifier.Text + "Tests" }, null).Project;
			}

			return project;
		}
	}
}
