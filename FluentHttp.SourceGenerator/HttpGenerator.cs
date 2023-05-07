using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FluentHttp.SourceGenerator
{
    /// <summary>
    ///  IIncrementalGenerator
    /// </summary>
    [Generator]
    public class HttpGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //var compilation = context.Compilation;
            //var mainMethod = compilation.GetEntryPoint(context.CancellationToken);
            var syntaxReceiver = (SyntaxReceiver)context.SyntaxReceiver;
            GenDapr(context, syntaxReceiver);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <summary>
        /// 生成Dapr
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syntaxReceiver"></param>
        private void GenDapr(GeneratorExecutionContext context, SyntaxReceiver syntaxReceiver)
        {
            List<RoslynSymbol> symbols = new List<RoslynSymbol>();
            syntaxReceiver.InterfaceDeclarationSyntaxes.ForEach(x =>
            {
                RoslynSymbol roslynSymbol = new RoslynSymbol
                {
                    InterfaceDeclarationSyntax = x
                };

                symbols.Add(roslynSymbol);
                SemanticModel model = context.Compilation.GetSemanticModel(x.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(x);
                // namepace test;
                var namespaceName = x.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name.ToString();
                if (string.IsNullOrWhiteSpace(namespaceName))
                {
                    namespaceName = classSymbol.ContainingNamespace.OriginalDefinition.ToDisplayString();
                }
                roslynSymbol.NamespaceName = namespaceName;
                // 获取接口上的注解
                var metaData = classSymbol.AttributeDict();
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
                    .WithNetNameSpace()
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
                    IMethodSymbol methodSymbol = model.GetDeclaredSymbol(memberDeclarationSyntax as MethodDeclarationSyntax);
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

            context.AddSource("ServiceCollectionExt.g.cs", SourceText.From(GenServiceAddDapr(symbols), Encoding.UTF8));
        }

        private string GenServiceAddDapr(List<RoslynSymbol> symbols)
        {
            var di = string.Empty;
            List<string> usings = new List<string>();
            symbols.ForEach(roslynSymbol =>
            {
                usings.AddRange(roslynSymbol.GetUsings);
                usings.Add(roslynSymbol.NamespaceName);
                di += $"services.TryAdd(ServiceDescriptor.Describe(typeof({roslynSymbol.InterfaceName}), typeof({roslynSymbol.ImplClassName}), lifetime));" + DaprConst.NewLine;
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
using System;
{us}
namespace YL.Extensions.DependencyInjection
{{
    public static partial class ServiceCollectionExtG
    {{
        public static IServiceCollection AddFluentHttp(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {{
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
            code += DaprConst.NewLine + $@"private readonly IContext _context;";
            code += DaprConst.NewLine + $@"private readonly IChooseUrl _chooseUrl;";
            return code;
        }

        private string GenConstruct(string name)
        {
            string code = $@"public {name} (IHttpClientFactory clientFactory, IContext context, IChooseUrl chooseUrl) {{
_clientFactory = clientFactory;
_context = context;
_chooseUrl = chooseUrl;
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
            if (memberSymbol.HttpMethodName == "PUT" || memberSymbol.HttpMethodName == "POST" || memberSymbol.HttpMethodName == "DELETE")
            {
                if (parameter is not null)
                {
                    tReqType = parameter.Type;
                    genericName = $"<{memberSymbol.TypeArgumentName}, {tReqType}>";
                }
            }

            var dapr = "var header = await _context.GetHeader(appId);" + DaprConst.NewLine;
            //            dapr += $@"
            //if(header is not null){{
            //  header.Add(""Auth"", ""{roslynSymbol.Auth}"");
            //}}
            //";
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
                dapr += $"var res = await client.Http{genericName}(_context, _chooseUrl, appId, url, {obj}, method, header, auth);";
            }
            else
            {
                switch (memberSymbol.HttpMethodName)
                {
                    //  IContext context, string appId, string url, Dictionary<string, object>? header = null
                    case "GET":
                        dapr += $"var res = await client.HttpGet{genericName}(_context, _chooseUrl, appId, url, header, auth);";
                        break;

                    case "POST":
                        dapr += $"var res = await client.HttpPost{genericName}(_context, _chooseUrl, appId, url, {obj}, header, auth);";
                        break;

                    case "DELETE":
                        if (string.IsNullOrWhiteSpace(obj))
                        {
                            dapr += $"var res = await client.HttpDelete{genericName}(_context, _chooseUrl, appId, url, header, auth);";
                        }
                        else
                        {
                            dapr += $"var res = await client.HttpDelete{genericName}(_context, _chooseUrl, appId, url, {obj}, header, auth);";
                        }
                        break;

                    case "HttpFileUpload":
                        var filepathParam = memberSymbol.Parameters.FirstOrDefault(c => c.IsFilePath);
                        var filepath = filepathParam.Name;
                        dapr += $"var res = await client.UploadFile{genericName}(_context, _chooseUrl, appId, url, {filepath}, header, auth);";
                        break;

                    case "PUT":
                        dapr += $"var res = await client.HttpPut{genericName}(_context, _chooseUrl, appId, url, {obj}, header, auth);";
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
    }
}