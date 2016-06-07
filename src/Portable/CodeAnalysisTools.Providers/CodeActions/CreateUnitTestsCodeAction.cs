using System.Collections.Generic;
using System.Collections.Immutable;
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
using Microsoft.CodeAnalysis.FindSymbols;

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

			model.PossibleAssemblies = this.document.Project.Solution.Projects.Where(x => x.Name.ToUpper().Contains("test".ToUpper())).Select(x => x.Name);
			model.SelectedAssembly = model.PossibleAssemblies.FirstOrDefault(x => x.ToUpper().Contains(currentAssembly.ToUpper())) ?? model.PossibleAssemblies.FirstOrDefault();

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

			

			var currentAssembly = this.document.Project.AssemblyName;
			var newAssembly = currentAssembly + ".UnitTests";

			if (model.CreaneNewAssembly)
			{
				var id = ProjectId.CreateNewId();
				var info = ProjectInfo.Create(id, VersionStamp.Create(), newAssembly, newAssembly + ".dll", LanguageNames.CSharp);

				var newSolution = this.document.Project.Solution.AddProject(info);
				this.document.Project.Solution.Workspace.TryApplyChanges(newSolution);
				project = newSolution.GetProject(id);
			}
			else
			{
				if (string.IsNullOrEmpty(model.SelectedAssembly))
				{
					return Enumerable.Empty<CodeActionOperation>();
				}

				project = this.document.Project.Solution.Projects.First(x => x.AssemblyName == model.SelectedAssembly);
			}
			var syntaxTree = await this.document.GetSyntaxTreeAsync(cancellationToken);
			var semanticModel = await this.document.GetSemanticModelAsync(cancellationToken);

			return await this.CreateDocuments(syntaxTree, semanticModel, project);
		}

		private async Task<IEnumerable<CodeActionOperation>> CreateDocuments(SyntaxTree syntaxTree, SemanticModel semanticModel, Project project)
		{
			var result = new List<CodeActionOperation>();
			var createdDocs = new List<Document>();

			var typeInfo = semanticModel.GetDeclaredSymbol(this.declaration);

			var allBaseClasses = new List<INamedTypeSymbol>() { typeInfo };

			// Ignore System.Object
			while (typeInfo.BaseType != null && typeInfo.BaseType.BaseType != null)
			{
				allBaseClasses.Add(typeInfo.BaseType);
				typeInfo = typeInfo.BaseType;
			}

			var allMethods = allBaseClasses.SelectMany(
				type => type.GetMembers()
					.OfType<IMethodSymbol>()
					.Where(x => x.DeclaredAccessibility == Accessibility.Public && x.IsOverride == false && x.IsImplicitlyDeclared == false));

			//var methods = this.declaration.Members.OfType<MethodDeclarationSyntax>().Where(x => x.Modifiers.Any(SyntaxKind.PublicKeyword));

			var newNamespaceName = SyntaxFactory.ParseName(project.AssemblyName + "." + this.declaration.Identifier.Text + "Tests");
			var folders = new[] { this.declaration.Identifier.Text + "Tests" };

			if (allMethods.Any(x => x.MethodKind == MethodKind.Constructor))
			{
				var constructorTestClass = this.CreateTestClass("Constructor");
				var newNamespace = SyntaxFactory.NamespaceDeclaration(newNamespaceName).AddMembers(constructorTestClass);
				var compilation = SyntaxFactory.CompilationUnit()
					.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.VisualStudio.TestTools.UnitTesting")))
					.AddMembers(newNamespace);

				var newDocument = project.AddDocument("Constructor_Should", compilation, folders, null);
				project = newDocument.Project;
				createdDocs.Add(newDocument);
			}

			foreach (var method in allMethods.Where(x => x.MethodKind == MethodKind.Ordinary))
			{
				var classDecl = this.CreateTestClass(method.Name);

				var newNamespace = SyntaxFactory.NamespaceDeclaration(newNamespaceName).AddMembers(classDecl);

				var compilation = SyntaxFactory.CompilationUnit()
					.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.VisualStudio.TestTools.UnitTesting")))
					.AddMembers(newNamespace);

				var newDocument = project.AddDocument(method.Name + "_Should", compilation, folders, null);
				project = newDocument.Project;
				createdDocs.Add(newDocument);
			}

			result.Add(new ApplyChangesOperation(project.Solution));
			if (createdDocs.Any())
			{
				result.Add(new OpenDocumentOperation(createdDocs.First().Id, true));
			}

			return result;
		}

		private ClassDeclarationSyntax CreateTestClass(string name)
		{
			return SyntaxFactory.ClassDeclaration(
					SyntaxFactory.Identifier(name + "_Should")
					.WithAdditionalAnnotations(RenameAnnotation.Create()))
					.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
					.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestClass")) })))
					.AddMembers(this.ScaffoldTestMethod());
		}

		private MethodDeclarationSyntax ScaffoldTestMethod()
		{
			var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "MyTestMethod")
				.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
				.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestMethod")) })))
				.WithBody(SyntaxFactory.Block().WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
					.WithLeadingTrivia(
						SyntaxFactory.TriviaList(
							SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Arrange."),
							SyntaxFactory.CarriageReturnLineFeed,
							SyntaxFactory.CarriageReturnLineFeed,
							SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Act."),
							SyntaxFactory.CarriageReturnLineFeed,
							SyntaxFactory.CarriageReturnLineFeed,
							SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "// Assert."),
							SyntaxFactory.CarriageReturn))));

			return method.WithIdentifier(method.Identifier.WithAdditionalAnnotations(RenameAnnotation.Create()));
		}
	}
}
