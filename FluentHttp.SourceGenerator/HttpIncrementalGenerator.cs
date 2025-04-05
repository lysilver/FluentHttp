using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace FluentHttp.SourceGenerator
{
    [Generator]
    public class HttpIncrementalGenerator : IIncrementalGenerator
    {
        public List<InterfaceDeclarationSyntax> InterfaceDeclarationSyntaxes { get; } = new List<InterfaceDeclarationSyntax>();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }

            IncrementalValuesProvider<ClassModel> provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "FluentHttp.Abstractions.FluentHttpAttribute",
            predicate: static (node, cancellationToken_) => node is ClassDeclarationSyntax,
            transform: static (ctx, cancellationToken) =>
            {
                ISymbol classSymbol = ctx.TargetSymbol;

                return new ClassModel(
                    classSymbol.Name,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    GetInterfaceModels(ctx.Attributes[0])
                    );
            });

            var incrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, _) =>
            {
                return OnVisitSyntaxNode(syntaxNode);
            },
                (GeneratorSyntaxContext generatorSyntaxContext, CancellationToken _) =>
                {
                    return generatorSyntaxContext;
                });
            string classNameSpace = "YL.Extensions.DependencyInjection";
            int Count = 0;
            List<RoslynSymbol> symbols = new();
            context.RegisterSourceOutput(incrementalValuesProvider, (spc, nameAndContent) =>
            {
                Count++;
                InterfaceDeclarationSyntaxes.Clear();
                OnVisitSyntaxNode(nameAndContent.Node);
                GenDapr(spc, nameAndContent, symbols);
            });
            var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
            var texts = context.AdditionalTextsProvider;

            var combined = texts.Combine(assemblyName);
            //context.RegisterSourceOutput(combined, (ctx, _) =>
            //{
            //    ctx.AddSource("ServiceCollectionExt.g.cs", SourceText.From(GenServiceAddDapr(symbols, classNameSpace), Encoding.UTF8));
            //});

            // get the additional text provider
            IncrementalValuesProvider<AdditionalText> additionalTexts = context.AdditionalTextsProvider;

            // apply a 1-to-1 transform on each text, extracting the path
            IncrementalValuesProvider<string> transformed = additionalTexts.Select(static (text, _) => text.Path);

            // collect the paths into a batch
            IncrementalValueProvider<ImmutableArray<string>> collected = transformed.Collect();

            symbols = symbols.Distinct(new RoslynSymbolCompare()).ToList();
            context.RegisterSourceOutput(collected, (sourceProductionContext, filePaths) =>
            {
                sourceProductionContext.AddSource("ServiceCollectionExt.g.cs", SourceText.From(GenServiceAddDapr(symbols, classNameSpace), Encoding.UTF8));
            });
        }

        private static EquatableList<InterfaceModel> GetInterfaceModels(AttributeData attribute)
        {
            EquatableList<InterfaceModel> ret = [];

            if (attribute.ConstructorArguments.Length == 0)
                return ret;

            foreach (TypedConstant constructorArgumentValue in attribute.ConstructorArguments[0].Values)
            {
                if (constructorArgumentValue.Value is INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceSymbol)
                {
                    EquatableList<string> properties = new();

                    foreach (IPropertySymbol interfaceProperty in interfaceSymbol
                        .GetMembers()
                        .OfType<IPropertySymbol>())
                    {
                        string type = interfaceProperty.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                        //Check if property has a setter
                        string setter = interfaceProperty.SetMethod is not null
                            ? "set; "
                            : string.Empty;

                        properties.Add($$"""
                            public {{type}} {{interfaceProperty.Name}} { get; {{setter}}}
                        """);
                    }

                    ret.Add(new InterfaceModel(interfaceSymbol.ToDisplayString(), properties));
                }
            }

            return ret;
        }

        public bool OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
                IsDapr(interfaceDeclarationSyntax))
            {
                InterfaceDeclarationSyntaxes.Add(interfaceDeclarationSyntax);
                return true;
            }
            return false;
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

        private static readonly string[] DaprAttributeString = { "FluentHttpAttribute", "FluentHttp" };

        private bool IsDapr(InterfaceDeclarationSyntax syntaxes)
        {
            return HasAttribute(syntaxes.AttributeLists, DaprAttributeString);
        }

        private void GenDapr(SourceProductionContext context, GeneratorSyntaxContext generatorSyntaxContext, List<RoslynSymbol> symbols)
        {
            string classNameSpace = "YL.Extensions.DependencyInjection";
            InterfaceDeclarationSyntaxes.ForEach(x =>
            {
                RoslynSymbol roslynSymbol = new RoslynSymbol
                {
                    InterfaceDeclarationSyntax = x
                };
                symbols.Add(roslynSymbol);

                var symbolDisplayFormat = new SymbolDisplayFormat
                        (
                            // 带上命名空间和类型名
                            SymbolDisplayGlobalNamespaceStyle.Included,
                            // 命名空间之前加上 global 防止冲突
                            SymbolDisplayTypeQualificationStyle
                                .NameAndContainingTypesAndNamespaces
                        );
                // SemanticModel model = generatorSyntaxContext.Compilation.GetSemanticModel(x.SyntaxTree);
                ISymbol classSymbol = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(x);

                var displayString = classSymbol.ToDisplayString(symbolDisplayFormat);

                // namepace test;
                var namespaceName = x.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name.ToString();
                if (string.IsNullOrWhiteSpace(namespaceName))
                {
                    namespaceName = classSymbol.ContainingNamespace.OriginalDefinition.ToDisplayString();
                }
                classNameSpace = namespaceName;
                roslynSymbol.NamespaceName = namespaceName;
                // 获取接口上的注解
                var metaData = classSymbol.SymbolAttributeDict();
                // 获取接口上的特性数据
                metaData.TryGetValue(DaprConst.Name, out string className);
                metaData.TryGetValue(DaprConst.Auth, out string auth);
                metaData.TryGetValue(DaprConst.AppId, out string appId);
                metaData.TryGetValue(DaprConst.Url, out string url);

                roslynSymbol.Auth = auth;
                roslynSymbol.AppId = appId;
                roslynSymbol.Url = url;
                roslynSymbol.ImplClassName = string.IsNullOrWhiteSpace(className) ? roslynSymbol.InterfaceName.Substring(1) : className;

                var sourceText = new SourceTextBuild()
                    .WithUsingNameSpaces(roslynSymbol.GetUsings)
                    .WithDaprNameSpace()
                    .WithNetNameSpace(!roslynSymbol.GetUsings.Contains("System.Net.Http"))
                    .WithNewLine()
                    .WithNameSpace(roslynSymbol.NamespaceName)
                    .WithLeftBracket()
                    .WithIndent(1)
                    .WithImpl("public", roslynSymbol.ImplClassName, roslynSymbol.InterfaceName)
                    .WithIndent(1)
                    .WithLeftBracket()
                    // 构造
                    .WithNewLine()
                    .WithIndent(2)
                    .WithContent(GenConstruct())
                    .WithNewLine()
                    .WithIndent(2)
                    .WithContent(GenConstruct(roslynSymbol.ImplClassName))
                    ;

                foreach (MemberDeclarationSyntax memberDeclarationSyntax in x.Members)
                {
                    IMethodSymbol methodSymbol = (IMethodSymbol)generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(memberDeclarationSyntax as MethodDeclarationSyntax);

                    MemberSymbol memberSymbol = new MemberSymbol
                    {
                        MemberDeclaration = memberDeclarationSyntax,
                        IMethodSymbol = methodSymbol
                    };
                    sourceText
                      .WithNewLine()
                      .WithIndent(2)
                      .WithMethod("public", $"{memberSymbol.ReturnMethodType}", memberSymbol.MethodName, GenParam(memberSymbol.Parameters))
                      .WithIndent(2).WithLeftBracket()
                      .WithIndent(3).WithContent(GenContext(roslynSymbol, memberSymbol))
                      .WithIndent(2).WithRightBracket();
                }
                sourceText.WithIndent(1)
                  .WithRightBracket()
                  .WithRightBracket()
                  ;
                // string str = sourceText.Build();
                //string code = GenDaprSourceText().Replace("{0}", roslynSymbol.NamespaceName);
                //code = code.Replace("{1}", roslynSymbol.ImplClassName);
                //string code2 = string.Format(GenDaprSourceText(), impLClassName);
                // context.AddSource(roslynSymbol.ImplClassName + ".g.cs", SourceText.From(code, Encoding.UTF8));
                context.AddSource(roslynSymbol.ImplClassName + ".g.cs", SourceText.From(sourceText.Build(), Encoding.UTF8));
            });
        }

        private string GenServiceAddDapr(List<RoslynSymbol> symbols, string classNameSpace)
        {
            var di = string.Empty;
            List<string> usings = new List<string>();
            //di += $"services.AddDefaultServiceDiscovery();" + DaprConst.NewLine;
            symbols.ForEach(roslynSymbol =>
            {
                usings.AddRange(roslynSymbol.GetUsings);
                usings.Add(roslynSymbol.NamespaceName);
                di += $"services.TryAdd(ServiceDescriptor.Describe(typeof({roslynSymbol.InterfaceName}), typeof({roslynSymbol.ImplClassName}), lifetime));" + DaprConst.NewLine + new string(' ', 12);
                //di += $"services.AddScoped<{roslynSymbol.InterfaceName}, {roslynSymbol.ImplClassName}>();" + System.Environment.NewLine;
            });
            var us = "";
            usings.Distinct().ToList().ForEach(c =>
            {
                us += $"using {c};" + DaprConst.NewLine;
            });
            string code =
    $@"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YL.Extensions.DependencyInjection;
using System;
{us}
namespace {classNameSpace}
{{
    public static partial class ServiceCollectionExtG
    {{
        public static IServiceCollection AddFluentHttp(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {{
            services.AddDefaultServiceDiscovery();
            {di}
            return services;
        }}
    }}
}}
          ";
            return code;
        }

        private string GenParam(List<Parameter> list)
        {
            string str = "";
            list.ForEach(c =>
            {
                str += $"{c.Type} {c.Name},";
            });
            return str.TrimEnd(',');
        }

        private string GenConstruct()
        {
            string code = $@"private readonly IHttpClientFactory _clientFactory;";
            //code += DaprConst.NewLine + $@"private readonly IContext _context;";
            //code += DaprConst.NewLine + $@"private readonly IChooseUrl _chooseUrl;";
            code += DaprConst.NewLine + $@"private readonly IHttpClientAdapter _clientAdapter;";
            return code;
        }

        private string GenConstruct(string name)
        {
            string code = $@"public {name} (IHttpClientFactory clientFactory, IContext context, IChooseUrl chooseUrl) {{
_clientFactory = clientFactory;
_context = context;
_chooseUrl = chooseUrl;
}}";
            code = $@"public {name} (IHttpClientFactory clientFactory, IHttpClientAdapter clientAdapter) {{
_clientFactory = clientFactory;
_clientAdapter = clientAdapter;
}}";
            return code;
        }

        private string GenContext(RoslynSymbol roslynSymbol, MemberSymbol memberSymbol)
        {
            var genericName = memberSymbol.TypeArgumentName;
            if (!string.IsNullOrWhiteSpace(genericName))
            {
                genericName = $"<{genericName}>";
            }
            var attributeDict = memberSymbol.AttributeDict;
            attributeDict.TryGetValue(DaprConst.Url, out var url);
            attributeDict.TryGetValue(DaprConst.AppId, out var appId);
            attributeDict.TryGetValue(DaprConst.ReturnString, out var returnString);
            attributeDict.TryGetValue(DaprConst.MemberHttpMethod, out string httpMethod);
            if (string.IsNullOrWhiteSpace(appId))
            {
                appId = roslynSymbol.AppId;
            }
            url = roslynSymbol.GetUrl(url);
            string url2 = "";
            string url3 = "";
            Dictionary<string, object> query = new Dictionary<string, object>();
            memberSymbol.Parameters.ForEach(c =>
            {
                if (c.IsPath)
                {
                    url2 += $"url = url.Replace(c.Name, {c.Name});";
                }
                else
                {
                    if (!c.IsFilePath)
                    {
                        query.Add(c.Name, c.Name);
                    }
                }
            });
            if (memberSymbol.HttpMethodName == "GET")
            {
                url3 = query.ToQueryString();
                if (!string.IsNullOrEmpty(url3))
                {
                    url3 = "?" + url3;
                    url += $"{url3}";
                }
            }
            var parameter = memberSymbol.Parameters.FirstOrDefault(c => !c.IsPath);
            var tReqType = "";
            if (memberSymbol.HttpMethodName == "Patch" || memberSymbol.HttpMethodName == "PUT" || memberSymbol.HttpMethodName == "POST" || memberSymbol.HttpMethodName == "DELETE")
            {
                if (parameter is not null)
                {
                    tReqType = parameter.Type;
                    genericName = $"<{memberSymbol.TypeArgumentName}, {tReqType}>";
                }
            }

            var dapr = "var header = await _context?.GetHeader(appId);" + DaprConst.NewLine;
            //            dapr += $@"
            //if(header is not null){{
            //  header.Add(""Auth"", ""{roslynSymbol.Auth}"");
            //}}
            //";
            dapr = "";
            var auth = roslynSymbol.Auth;
            var obj = parameter?.Name;
            if (bool.TryParse(returnString, out bool flag) && flag)
            {
                obj ??= "null";
                var method = memberSymbol.HttpMethodName;
                if (method == "HttpFileUpload")
                {
                    method = DaprConst.Post;
                }
                dapr += $@"var method = $""{method}"";";
                dapr += DaprConst.NewLine;
                dapr += $"var res = await client.Http{genericName}(_clientAdapter, appId, url, {obj}, method, auth);";
            }
            else
            {
                switch (memberSymbol.HttpMethodName)
                {
                    //  IContext context, string appId, string url, Dictionary<string, object>? header = null
                    case "GET":
                        dapr += $"var res = await client.HttpGet{genericName}(_clientAdapter, appId, url, auth);";
                        break;

                    case "POST":
                        dapr += $"var res = await client.HttpPost{genericName}(_clientAdapter, appId, url, {obj}, auth);";
                        break;

                    case "DELETE":
                        if (string.IsNullOrWhiteSpace(obj))
                        {
                            dapr += $"var res = await client.HttpDelete{genericName}(_clientAdapter, appId, url, auth);";
                        }
                        else
                        {
                            dapr += $"var res = await client.HttpDelete{genericName}(_clientAdapter, appId, url, {obj}, auth);";
                        }
                        break;

                    case "HttpFileUpload":
                        var filepathParam = memberSymbol.Parameters.FirstOrDefault(c => c.IsFilePath);
                        var formDataParam = memberSymbol.Parameters.FirstOrDefault(c => !c.IsFilePath);
                        var filepath = filepathParam.Name;
                        if (formDataParam is not null)
                        {
                            dapr += $"var res = await client.UploadFile{genericName}(_clientAdapter, appId, url, {filepath}, auth, {formDataParam.Name});";
                        }
                        else
                        {
                            dapr += $"var res = await client.UploadFile{genericName}(_clientAdapter, appId, url, {filepath}, auth);";
                        }
                        break;

                    case "PUT":
                        dapr += $"var res = await client.HttpPut{genericName}(_clientAdapter, appId, url, {obj}, auth);";
                        break;

                    case "Patch":
                        dapr += $"var res = await client.HttpPatch{genericName}(_clientAdapter, appId, url, {obj}, auth);";
                        break;

                    case "HttpResponseMessage":
                        obj ??= "null";
                        genericName = $"<object>";
                        if (parameter is not null)
                        {
                            tReqType = parameter.Type;
                            genericName = $"<{tReqType}>";
                        }
                        dapr += "var method = " + $@"""{GetMethod(httpMethod)}""" + ";" + DaprConst.NewLine;
                        dapr += $"var res = await client.HttpResponseMessageAsync{genericName}(_clientAdapter, appId, url, {obj}, method, auth);";
                        break;

                    default:
                        break;
                }
            }

            string code = $@"
            var appId = ""{appId}"";
            var url = $""{url}"";
            var auth = $""{auth}"";
            using var client = _clientFactory.CreateClient();
            {dapr}
            return res;
            ";
            return code;
        }

        private string GetMethod(string method)
        {
            // Post, Delete, Put, Patch, Get
            return method switch
            {
                "0" => "POST",
                "1" => "DELETE",
                "2" => "PUT",
                "3" => "PATCH",
                _ => "GET",
            };
        }
    }
}