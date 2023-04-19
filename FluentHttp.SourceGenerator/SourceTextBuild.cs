using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Text;

namespace FluentHttp.SourceGenerator
{
    public class SourceTextBuild
    {
        private readonly StringBuilder stringBuilder;
        private readonly string SingleIndent = new string(' ', 4);

        public SourceTextBuild()
        {
            stringBuilder = new StringBuilder();
        }

        public void AppendIndent(int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                stringBuilder.Append(SingleIndent);
            }
        }

        public string Build()
        {
            return CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot().NormalizeWhitespace()
                .SyntaxTree.GetText().ToString()
             ;
        }

        public SourceTextBuild WithIndent(int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                stringBuilder.Append(SingleIndent);
            }
            return this;
        }

        public SourceTextBuild WithUsingNameSpace(string namespaceName)
        {
            stringBuilder.AppendLine($"using {namespaceName};");
            return this;
        }

        public SourceTextBuild WithDaprNameSpace()
        {
            stringBuilder.AppendLine("using FluentHttp.Ext;");
            stringBuilder.AppendLine("using FluentHttp.Ext.LoadBalancing;");
            return this;
        }

        public SourceTextBuild WithNetNameSpace()
        {
            stringBuilder.AppendLine("using System.Net.Http;");
            return this;
        }

        public SourceTextBuild WithUsingNameSpaces(List<string> list)
        {
            list.ForEach(x =>
            {
                WithUsingNameSpace(x);
            });
            return this;
        }

        public SourceTextBuild WithNameSpace(string nameSpaceName)
        {
            stringBuilder.AppendLine($"namespace {nameSpaceName}");
            return this;
        }

        public SourceTextBuild WithNewLine()
        {
            stringBuilder.Append(DaprConst.NewLine);
            return this;
        }

        public SourceTextBuild WithLeftBracket()
        {
            stringBuilder.AppendLine("{");
            return this;
        }

        public SourceTextBuild WithRightBracket()
        {
            stringBuilder.AppendLine("}");
            return this;
        }

        public SourceTextBuild WithClass(string modifier, string className)
        {
            stringBuilder.AppendLine($"{modifier} class {className}");
            return this;
        }

        public SourceTextBuild WithInterface(string modifier, string interfaceName)
        {
            stringBuilder.AppendLine($"{modifier} interface {interfaceName}");
            return this;
        }

        public SourceTextBuild WithImpl(string modifier, string className, string interfaceName)
        {
            stringBuilder.AppendLine($"{modifier} class {className} : {interfaceName}");
            return this;
        }

        public SourceTextBuild WithMethod(string modifier, string returnType, string methodName, string param)
        {
            stringBuilder.AppendLine($"{modifier} {returnType} {methodName}({param})");
            return this;
        }

        public SourceTextBuild WithContent(string text)
        {
            stringBuilder.AppendLine($@"{text}");
            return this;
        }
    }
}