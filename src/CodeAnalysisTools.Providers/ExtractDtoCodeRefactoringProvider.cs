using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisTools.Refactorings
{
	public class ExtractDtoCodeRefactoringProvider : CodeAnalysisRefactoringProvider
	{
		public const string RefactoringId = "REF001";
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

						publicMembers = publicMembers
							.Add(
								property.WithAccessorList(
									SyntaxFactory.AccessorList(
										property.AccessorList.OpenBraceToken,
										SyntaxFactory.List(property.AccessorList.Accessors.Where(a => isInterface || a.Modifiers.Any(SyntaxKind.PublicKeyword))),
										property.AccessorList.CloseBraceToken))
										.WithModifiers(
									SyntaxFactory.TokenList(
										SyntaxFactory.Token(isInterface ? SyntaxKind.PublicKeyword : SyntaxKind.None))));
						break;
					case SyntaxKind.MethodDeclaration:
						if (options.ImplementMethods == false)
						{
							continue;
						}
						var method = member as MethodDeclarationSyntax;
						publicMembers = publicMembers
							.Add(
								method
									.WithBody(SyntaxFactory.Block()
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
						break;
				}
			}

			var compilation = typeDecl.SyntaxTree.GetCompilationUnitRoot(cancellationToken);

			var classDecl = SyntaxFactory.ClassDeclaration((string)dtoName)
								.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
								.WithTypeParameterList((TypeParameterListSyntax)typeDecl.TypeParameterList)
								.WithMembers(publicMembers);

			var oldNameSpace = typeDecl.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();

			var newNamespaceName = SyntaxFactory.QualifiedName(oldNameSpace.Name.WithoutTrailingTrivia(), SyntaxFactory.IdentifierName("Dtos"));
			var newNameSpace = SyntaxFactory.NamespaceDeclaration(newNamespaceName)
				.AddMembers(classDecl);

			var syntaxRoot = SyntaxFactory.CompilationUnit()
				.WithUsings(compilation.Usings)
				.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
				.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>().Add(newNameSpace));

			var originalSolution = document.Project.Solution;
			var optionSet = originalSolution.Workspace.Options;

			return document.Project.AddDocument(dtoName, syntaxRoot, document.Folders).Project.Solution;
		}
	}
}