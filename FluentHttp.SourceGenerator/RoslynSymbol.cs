using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHttp.SourceGenerator
{
    #region RoslynSymbol

    public class RoslynSymbol
    {
        public string NamespaceName
        {
            get; set;
        }

        /// <summary>
        /// 接口名称
        /// </summary>
        public string InterfaceName
        {
            get
            {
                return InterfaceDeclarationSyntax.Identifier.ValueText;
            }
        }

        /// <summary>
        /// 实现类名称
        /// </summary>
        public string ImplClassName
        {
            get; set;
        }

        /// <summary>
        /// 认证获取的key
        /// </summary>
        public string Auth
        {
            get; set;
        }

        public string AppId
        {
            get; set;
        }

        /// <summary>
        /// 接口信息
        /// </summary>
        public InterfaceDeclarationSyntax InterfaceDeclarationSyntax { get; set; }

        /// <summary>
        /// 接口定义方法信息
        /// </summary>
        public List<MemberSymbol> MemberSymbols { get; set; } = new List<MemberSymbol>();

        public List<string> GetUsings
        {
            get
            {
                return InterfaceDeclarationSyntax.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault()?.GetUsings();
            }
        }
    }

    #endregion RoslynSymbol

    #region MemberSymbol

    public class MemberSymbol
    {
        public MemberDeclarationSyntax MemberDeclaration { get; set; }

        public MethodDeclarationSyntax MethodDeclarationSyntax
        {
            get
            {
                return MemberDeclaration as MethodDeclarationSyntax;
            }
        }

        public IMethodSymbol IMethodSymbol { get; set; }

        /// <summary>
        /// 特性上的参数
        /// </summary>
        public Dictionary<string, string> AttributeDict
        {
            get
            {
                var metaData = IMethodSymbol.GetAttributes()
                   .Where(x => !string.IsNullOrEmpty(x.AttributeClass?.Name))
                   .Where(x => x.NamedArguments.Length > 0)
                   .SelectMany(x => x.NamedArguments)
                   .ToDictionary(x => x.Key, x => x.Value.Value.ToString());
                return metaData;
            }
        }

        public Dictionary<string, string> HttpMethod
        {
            get
            {
                return DaprConst.HttpMethod;
            }
        }

        /// <summary>
        /// path变量的
        /// </summary>
        public Dictionary<string, string> PathDict
        {
            get
            {
                var metaData = IMethodSymbol.Parameters
                    .SelectMany(x => x.GetAttributes())
                   .Where(x => !string.IsNullOrEmpty(x.AttributeClass?.Name) &&
                   x.AttributeClass.ToString().Equals(HttpMethod["PATH"]))
                   .Where(x => x.NamedArguments.Length > 0)
                   .SelectMany(x => x.NamedArguments)
                   .ToDictionary(x => x.Key, x => (string)x.Value.Value);
                return metaData;
            }
        }

        /// <summary>
        /// 方法的参数
        /// </summary>
        public List<Parameter> Parameters
        {
            get
            {
                var parameters = IMethodSymbol.Parameters
                    .Select(c => new Parameter
                    {
                        Name = c.Name,
                        Type = c.Type.ToString(),
                        IsPath = c.GetAttributes()
                               .Any(x => !string.IsNullOrEmpty(x.AttributeClass?.Name) &&
                               x.AttributeClass.ToString().Equals(HttpMethod["PATH"])),
                        IsFilePath = c.GetAttributes()
                               .Any(x => !string.IsNullOrEmpty(x.AttributeClass?.Name) &&
                               x.AttributeClass.ToString().Equals(HttpMethod["FilePath"]))
                    }).ToList();
                return parameters;
            }
        }

        public string AttributeName => MethodDeclarationSyntax
            .AttributeLists.FirstOrDefault()?
            .Attributes.FirstOrDefault()?
            .Name?.ToFullString();

        public string HttpMethodName
        {
            get
            {
                var arr = HttpMethod.Values.ToArray();
                //var methodAttribute = IMethodSymbol.GetAttributes()
                //     .Where(x => !string.IsNullOrEmpty(x.AttributeClass?.Name))
                //     .FirstOrDefault().AttributeClass.ToString();

                var methodAttribute = IMethodSymbol.GetAttributes()
                     .Where(x => !string.IsNullOrEmpty(x.AttributeClass?.Name)
                     && arr.Contains(x.AttributeClass.ToString()))
                     .FirstOrDefault()?.AttributeClass.ToString();
                return HttpMethod.FirstOrDefault(c => c.Value == methodAttribute).Key ?? "GET";
            }
        }

        public string MethodName
        {
            get
            {
                return IMethodSymbol.Name;
            }
        }

        public bool IsAsync
        {
            get
            {
                return IMethodSymbol.ReturnType.Name == "Task";
                // return IMethodSymbol.IsAsync;
            }
        }

        public string ReturnType
        {
            get
            {
                return IMethodSymbol.ReturnType.ToString();
            }
        }

        public string ReturnMethodType
        {
            get
            {
                return IsAsync ? $"async {ReturnType}" : $"{ReturnType}";
            }
        }

        public string TypeArgumentName
        {
            get
            {
                var type = ((GenericNameSyntax)MethodDeclarationSyntax.ReturnType).TypeArgumentList;
                var str = type.Arguments.FirstOrDefault().ToString();
                return str;
            }
        }
    }

    #endregion MemberSymbol

    #region Parameter

    public class Parameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否Path变量
        /// </summary>
        public bool IsPath { get; set; }

        /// <summary>
        /// 是否是文件路径
        /// </summary>
        public bool IsFilePath { get; set; }
    }

    #endregion Parameter
}