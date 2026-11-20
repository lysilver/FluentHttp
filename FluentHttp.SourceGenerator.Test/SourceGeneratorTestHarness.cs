using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FluentHttp.SourceGenerator.Test
{
    public sealed class SourceGeneratorTestHarness
    {
        private readonly List<ISourceGenerator> _generators = [];
        private readonly List<AdditionalText> _additionalTexts = [];
        private readonly List<Type> _extraTypes = [];
        private readonly List<string> _source = [];
        private readonly Dictionary<string, string> _globalOptions = [];
        private CSharpParseOptions _parseOptions = CSharpParseOptions.Default;
        private string _assemblyName = "TestAssembly";

        public SourceGeneratorTestHarness() { }

        public SourceGeneratorTestHarness(string source)
        {
            _source.Add(source);
        }

        public SourceGeneratorTestHarness AddGenerator<TGenerator>() where TGenerator : IIncrementalGenerator, new()
        {
            var generator = new TGenerator().AsSourceGenerator();
            _generators.Add(generator);
            return this;
        }

        public SourceGeneratorTestHarness AddGenerator(IIncrementalGenerator generator)
        {
            _generators.Add(generator.AsSourceGenerator());
            return this;
        }

        public SourceGeneratorTestHarness AddSourceText(string source)
        {
            _source.Add(source);
            return this;
        }

        public SourceGeneratorTestHarness AddSourceText(params string[] source)
        {
            _source.AddRange(source);
            return this;
        }

        public SourceGeneratorTestHarness AddAdditionalText(AdditionalText text)
        {
            _additionalTexts.Add(text);
            return this;
        }

        public SourceGeneratorTestHarness AddAdditionalText(IEnumerable<AdditionalText>? texts)
        {
            if (texts is null)
            {
                return this;
            }
            _additionalTexts.AddRange(texts);
            return this;
        }

        public SourceGeneratorTestHarness AddReferenceType(Type type)
        {
            _extraTypes.Add(type);
            return this;
        }

        public SourceGeneratorTestHarness AddGlobalOption(string propertyName, string value)
        {
            _globalOptions[$"build_property.{propertyName}"] = value;
            return this;
        }

        public SourceGeneratorTestHarness WithLanguageVersion(LanguageVersion version = LanguageVersion.Latest)
        {
            _parseOptions = _parseOptions.WithLanguageVersion(version);
            return this;
        }

        public SourceGeneratorTestHarness WithParseOptions(Func<CSharpParseOptions, CSharpParseOptions> configure)
        {
            _parseOptions = configure(_parseOptions);
            return this;
        }

        public SourceGeneratorTestHarness WithAssemblyName(string assemblyName)
        {
            _assemblyName = assemblyName;
            return this;
        }

        public GeneratorDriverRunResult Run()
        {
            (GeneratorDriverRunResult result, _) = RunCompilation();
            return result;
        }

        public (GeneratorDriverRunResult result, CSharpCompilation compilation) RunCompilation()
        {
            var references = BuildMetadataReference(_extraTypes);
            var compilation = BuildCSharpCompilation(_source, references);

            GeneratorDriver driver = CSharpGeneratorDriver.Create(
                generators: _generators,
                parseOptions: _parseOptions,
                additionalTexts: _additionalTexts,
                optionsProvider: new TestAnalyzerConfigOptionsProvider(_globalOptions)
            );
            driver = driver.RunGenerators(compilation);
            return (driver.GetRunResult(), compilation);
        }

        private static List<MetadataReference> BuildMetadataReference(IEnumerable<Type> types)
        {
            var references = BuildMetadataReference([.. types]);
            references.AddRange(GetMetadataReferences());
            return references;
        }

        public CSharpCompilation BuildCSharpCompilation(string source, IEnumerable<MetadataReference> references)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, _parseOptions);
            return CSharpCompilation.Create(_assemblyName,
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        public CSharpCompilation BuildCSharpCompilation(List<string> source, IEnumerable<MetadataReference> references)
        {
            var syntaxTrees = source.Select(c => CSharpSyntaxTree.ParseText(c, _parseOptions));
            return CSharpCompilation.Create(_assemblyName,
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        public static List<MetadataReference> BuildMetadataReference(params Type[] types)
        {
            var coreLib = typeof(object).Assembly.Location;
            var runtimeDir = Path.GetDirectoryName(coreLib)!;

            List<MetadataReference> references = [
                   MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                   MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                   MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                   MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                   MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                   MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "netstandard.dll")),
                   MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Collections.dll")),
                   MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Linq.dll"))
                ];
            if (types is not null)
            {
                foreach (var item in types)
                {
                    var path = item.Assembly.Location;
                    if (!references.Any(r => r.Display == path))
                    {
                        references.Add(MetadataReference.CreateFromFile(path));
                    }
                }
            }
            return references;
        }

        public static IEnumerable<MetadataReference> GetMetadataReferences()
        {
            var trustedPlatformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

            return trustedPlatformAssemblies
                .Split(Path.PathSeparator)
                .Select(static path => MetadataReference.CreateFromFile(path));
        }
    }

}
