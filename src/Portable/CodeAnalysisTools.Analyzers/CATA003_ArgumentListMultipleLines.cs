using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ArgumentListMultipleLinesAnalyzer : SyntaxNodeAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(Rule);
			}
		}

		public override ImmutableArray<SyntaxKind> SupportedKinds
		{
			get
			{
				return ImmutableArray.Create(SyntaxKind.ArgumentList);
			}
		}

		public override string DiagnosticId
		{
			get
			{
				return "CATA003";
			}
		}

		public override string Title
		{
			get
			{
				return "If method parameters span multiple lines, all parameters must be on a separate line.";
			}
		}

		public override string Description
		{
			get
			{
				return "If method parameters span multiple lines, all parameters must be on a separate line.";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "If method parameters span multiple lines, all parameters must be on a separate line.";
			}
		}

		public override string Category
		{
			get
			{
				return "Readability";
			}
		}

		public async override Task AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			var argumentList = (ArgumentListSyntax)context.Node;

			bool hasEndLineOnOpenBrace = argumentList.OpenParenToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);

			var lineSpan = argumentList.GetLocation().GetLineSpan();
			bool isMultiLine = lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;

			bool hasArgumentsOnSameLine = argumentList.Arguments.GetSeparators().Any(x => x.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false && x.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false);

			if ((isMultiLine && (hasArgumentsOnSameLine || hasEndLineOnOpenBrace == false)) || (hasArgumentsOnSameLine && hasEndLineOnOpenBrace))
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, argumentList.GetLocation()));
			}
		}
	}
}
