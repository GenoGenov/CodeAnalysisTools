using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace CodeAnalysisTools.Refactorings
{
	public class ExtractDtoCodeRefactoringProvider : CodeAnalysisRefactoringProvider
	{
		public const string RefactoringName = "Extract DTO";
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, CodeAnalysisOptions options)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);

			var decl = node as TypeDeclarationSyntax;
			if (decl != null)
			{
				var action = CodeAction.Create(RefactoringName, c => ExtractDtoAsync(context.Document, decl, c, options));

				context.RegisterRefactoring(action);
			}
		}

		private async Task<Solution> ExtractDtoAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken, CodeAnalysisOptions options)
		{
			Project project = null;

			if (string.IsNullOrEmpty(options.ExtractDto.Project) == false)
			{
				project = document.Project.Solution.Projects.FirstOrDefault(x => x.Name == options.ExtractDto.Project);
			}
			else
			{
				project = document.Project;
			}

			var identifierToken = typeDecl.Identifier;
			var identifierText = identifierToken.Text;
			bool isInterface = typeDecl.IsKind(SyntaxKind.InterfaceDeclaration);
			if (isInterface && typeDecl.Identifier.Text.StartsWith("I"))
			{
				identifierText = identifierText.Substring(1);
			}

			var dtoName = identifierText + "Dto";

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
			var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

			var publicMembers = SyntaxFactory.List<MemberDeclarationSyntax>();

			foreach (var member in typeDecl.Members)
			{
				switch (member.Kind())
				{
					case SyntaxKind.PropertyDeclaration:
						var property = member as PropertyDeclarationSyntax;
						if (isInterface == false && property.Modifiers.Any(SyntaxKind.PublicKeyword) == false)
							continue;

						var newProperty = property.WithAccessorList(
									SyntaxFactory.AccessorList(
										property.AccessorList.OpenBraceToken,
										SyntaxFactory.List(
														  new[] { SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
																  SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))}),
										property.AccessorList.CloseBraceToken));

						if (isInterface)
						{
							newProperty = newProperty.WithModifiers(
																   SyntaxFactory.TokenList(
																	   property.Modifiers.Add(
																		   SyntaxFactory.Token(SyntaxKind.PublicKeyword))));
						}

						publicMembers = publicMembers.Add(newProperty);
						break;
					case SyntaxKind.MethodDeclaration:
						if (options.ExtractDto.ImplementMethods == false)
						{
							continue;
						}
						var method = member as MethodDeclarationSyntax;
						if (isInterface || method.Modifiers.Any(SyntaxKind.AbstractKeyword))
						{
							publicMembers = publicMembers
							.Add(
								method
									.WithBody(
											 SyntaxFactory.Block()
										.WithStatements(
											SyntaxFactory.SingletonList<StatementSyntax>(
												SyntaxFactory.ThrowStatement(
													SyntaxFactory.ObjectCreationExpression(
														SyntaxFactory.IdentifierName("NotImplementedException"))
															.WithArgumentList(SyntaxFactory.ArgumentList())))))
									.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
									.WithModifiers(
										SyntaxFactory.TokenList(
											SyntaxFactory.Token(isInterface ? SyntaxKind.PublicKeyword : SyntaxKind.None))));
						}
						break;
				}
			}

			var compilation = typeDecl.SyntaxTree.GetCompilationUnitRoot(cancellationToken);

			var classDecl = SyntaxFactory.ClassDeclaration(dtoName)
								.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
								.WithTypeParameterList((TypeParameterListSyntax)typeDecl.TypeParameterList)
								.WithMembers(publicMembers);

			var oldNameSpace = typeDecl.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
			var newNamespaceName = this.GetNewNamespaceName(options, project);
			var newNameSpace = SyntaxFactory.NamespaceDeclaration(newNamespaceName)
				.AddMembers(classDecl);

			var syntaxRoot = SyntaxFactory.CompilationUnit()
				.WithUsings(
				SyntaxFactory.List(
					compilation.Usings
					.Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
					.Add(SyntaxFactory.UsingDirective(oldNameSpace.Name))
					.Select(x => x.WithAdditionalAnnotations(Simplifier.Annotation))))
				.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>().Add(newNameSpace));

			var originalSolution = document.Project.Solution;

			return project.AddDocument(dtoName, syntaxRoot, options.ExtractDto.Folders ?? document.Folders).Project.Solution;
		}

		private NameSyntax GetNewNamespaceName(CodeAnalysisOptions options, Project project)
		{
			var newNamespaceName = (options.ExtractDto.DefaultNamespace ?? project.Name);
			if (options.ExtractDto.Folders != null && options.ExtractDto.Folders.Any())
			{
				newNamespaceName += "." + string.Join(".", options.ExtractDto.Folders);
			}

			return SyntaxFactory.ParseName(newNamespaceName);
		}
	}
}