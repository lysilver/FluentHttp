using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHttp.SourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
        public List<InterfaceDeclarationSyntax> InterfaceDeclarationSyntaxes { get; } = new List<InterfaceDeclarationSyntax>();
        public List<MemberDeclarationSyntax> MemberSyntaxes { get; } = new List<MemberDeclarationSyntax>();

        private static readonly string[] DaprAttributeString = { "FluentHttpAttribute", "FluentHttp" };

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
                IsDapr(interfaceDeclarationSyntax))
            {
                InterfaceDeclarationSyntaxes.Add(interfaceDeclarationSyntax);
            }
        }

        private bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string[] attributeName)
        {
            return attributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => attributeName.Contains(GetSimpleNameFromNode(a).Identifier.ValueText));
        }

        private SimpleNameSyntax GetSimpleNameFromNode(AttributeSyntax node)
        {
            var identifierNameSyntax = node.Name as IdentifierNameSyntax;
            var qualifiedNameSyntax = node.Name as QualifiedNameSyntax;

            return
                identifierNameSyntax
                ??
                qualifiedNameSyntax?.Right
                ??
                (node.Name as AliasQualifiedNameSyntax).Name;
        }

        private bool IsDapr(InterfaceDeclarationSyntax syntaxes)
        {
            return HasAttribute(syntaxes.AttributeLists, DaprAttributeString);
        }
    }
}