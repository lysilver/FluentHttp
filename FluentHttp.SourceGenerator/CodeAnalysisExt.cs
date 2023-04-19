using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace FluentHttp.SourceGenerator
{
    public static class CodeAnalysisExt
    {
        public static string GetNamedParamValue(this AttributeData attributeData, string paramName)
        {
            var pair = attributeData.NamedArguments.FirstOrDefault(x => x.Key == paramName);
            return pair.Value.Value?.ToString();
        }

        public static CompilationUnitSyntax GetCompilationUnit(this SyntaxNode syntaxNode) =>
            syntaxNode.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();

        public static List<string> GetUsings(this CompilationUnitSyntax root) => root.ChildNodes()
               .OfType<UsingDirectiveSyntax>()
               .Select(n => n.Name.ToString())
               .ToList();

        public static bool IsVoid(this TypeSyntax typeSyntax)
        {
            return typeSyntax is PredefinedTypeSyntax t && t.Keyword.IsKind(SyntaxKind.VoidKeyword);
        }

        public static Dictionary<string, string> AttributeDict(this INamedTypeSymbol namedTypeSymbol)
        {
            var metaData = namedTypeSymbol.GetAttributes()
                  .Where(x => !string.IsNullOrEmpty(x.AttributeClass?.Name))
                  .Where(x => x.NamedArguments.Length > 0)
                  .SelectMany(x => x.NamedArguments)
                  .ToDictionary(x => x.Key, x => (string)x.Value.Value);
            return metaData;
        }
    }
}