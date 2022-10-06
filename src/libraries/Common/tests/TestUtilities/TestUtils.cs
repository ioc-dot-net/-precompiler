using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace LibraryImportGenerator.UnitTests
{
    /// <summary>
    /// The target framework to compile against.
    /// </summary>
    /// <remarks>
    /// This enumeration is for testing only and is not to be confused with the product's TargetFramework enum.
    /// </remarks>
    public enum TestTargetFramework
    {
        /// <summary>
        /// The latest supported .NET Framework version.
        /// </summary>
        Framework,
        /// <summary>
        /// The latest supported .NET Core version.
        /// </summary>
        Core,
        /// <summary>
        /// The latest supported .NET Standard version.
        /// </summary>
        Standard,
        /// <summary>
        /// The latest supported (live-built) .NET version.
        /// </summary>
        Net,
        /// <summary>
        /// .NET version 5.0.
        /// </summary>
        Net5,
        /// <summary>
        /// .NET version 6.0.
        /// </summary>
        Net6,
    }

    public static class TestUtils
    {
        internal static string GetFileLineName(
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? filePath = null)
            => $"{Path.GetFileName(filePath)}:{lineNumber}";

        internal static void Use<T>(T _)
        {
            // Workaround for - xUnit1026 // Theory methods should use all of their parameters
        }

        /// <summary>
        /// Disable binding redirect warnings. They are disabled by default by the .NET SDK, but not by Roslyn.
        /// See https://github.com/dotnet/roslyn/issues/19640.
        /// </summary>
        internal static ImmutableDictionary<string, ReportDiagnostic> BindingRedirectWarnings { get; } = new Dictionary<string, ReportDiagnostic>()
            {
                { "CS1701", ReportDiagnostic.Suppress },
                { "CS1702", ReportDiagnostic.Suppress },
            }.ToImmutableDictionary();

        /// <summary>
        /// Assert the pre-source generator compilation has only
        /// the expected failure diagnostics.
        /// </summary>
        /// <param name="comp"></param>
        public static void AssertPreSourceGeneratorCompilation(Compilation comp, params string[] additionalAllowedDiagnostics)
        {
            var allowedDiagnostics = new HashSet<string>()
            {
                "CS8795", // Partial method impl missing
                "CS0234", // Missing type or namespace - LibraryImportAttribute
                "CS0246", // Missing type or namespace - LibraryImportAttribute
                "CS8019", // Unnecessary using
            };

            foreach (string diagnostic in additionalAllowedDiagnostics)
            {
                allowedDiagnostics.Add(diagnostic);
            }

            var compDiags = comp.GetDiagnostics();
            Assert.All(compDiags, diag =>
            {
                Assert.Subset(allowedDiagnostics, new HashSet<string> { diag.Id });
            });
        }

        /// <summary>
        /// Assert the post-source generator compilation has only
        /// the expected failure diagnostics.
        /// </summary>
        /// <param name="comp"></param>
        public static void AssertPostSourceGeneratorCompilation(Compilation comp, params string[] additionalAllowedDiagnostics)
        {
            var allowedDiagnostics = new HashSet<string>()
            {
                "CS8019", // Unnecessary using
            };

            foreach (string diagnostic in additionalAllowedDiagnostics)
            {
                allowedDiagnostics.Add(diagnostic);
            }

            var compDiags = comp.GetDiagnostics();
            Assert.All(compDiags, diag =>
            {
                Assert.Subset(allowedDiagnostics, new HashSet<string> { diag.Id });
            });
        }

        /// <summary>
        /// Create a compilation given source
        /// </summary>
        /// <param name="source">Source to compile</param>
        /// <param name="targetFramework">Target framework of the compilation</param>
        /// <param name="outputKind">Output type</param>
        /// <param name="refs">Additional metadata references</param>
        /// <param name="preprocessorSymbols">Prepocessor symbols</param>
        /// <param name="allowUnsafe">Indicate if the compilation should allow unsafe code blocks</param>
        /// <returns>The resulting compilation</returns>
        public static Task<Compilation> CreateCompilation(string source, TestTargetFramework targetFramework = TestTargetFramework.Net, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, IEnumerable<MetadataReference>? refs = null, IEnumerable<string>? preprocessorSymbols = null, bool allowUnsafe = true)
        {
            return CreateCompilation(new[] { source }, targetFramework, outputKind, refs, preprocessorSymbols, allowUnsafe);
        }

        /// <summary>
        /// Create a compilation given sources
        /// </summary>
        /// <param name="sources">Sources to compile</param>
        /// <param name="targetFramework">Target framework of the compilation</param>
        /// <param name="outputKind">Output type</param>
        /// <param name="refs">Additional metadata references</param>
        /// <param name="preprocessorSymbols">Prepocessor symbols</param>
        /// <param name="allowUnsafe">Indicate if the compilation should allow unsafe code blocks</param>
        /// <returns>The resulting compilation</returns>
        public static Task<Compilation> CreateCompilation(string[] sources, TestTargetFramework targetFramework = TestTargetFramework.Net, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, IEnumerable<MetadataReference>? refs = null, IEnumerable<string>? preprocessorSymbols = null, bool allowUnsafe = true)
        {
            return CreateCompilation(
                sources.Select(source =>
                    CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: preprocessorSymbols))).ToArray(),
                targetFramework,
                outputKind,
                refs,
                allowUnsafe);
        }

        /// <summary>
        /// Create a compilation given sources
        /// </summary>
        /// <param name="sources">Sources to compile</param>
        /// <param name="targetFramework">Target framework of the compilation</param>
        /// <param name="outputKind">Output type</param>
        /// <param name="refs">Additional metadata references</param>
        /// <param name="allowUnsafe">Indicate if the compilation should allow unsafe code blocks</param>
        /// <returns>The resulting compilation</returns>
        public static async Task<Compilation> CreateCompilation(SyntaxTree[] sources, TestTargetFramework targetFramework = TestTargetFramework.Net, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, IEnumerable<MetadataReference>? refs = null, bool allowUnsafe = true)
        {
            var referenceAssemblies = await GetReferenceAssemblies(targetFramework);

            if (refs is not null)
            {
                referenceAssemblies = referenceAssemblies.AddRange(refs);
            }

            return CSharpCompilation.Create("compilation",
                sources,
                referenceAssemblies,
                new CSharpCompilationOptions(outputKind, allowUnsafe: allowUnsafe, specificDiagnosticOptions: BindingRedirectWarnings));
        }

        /// <summary>
        /// Get the reference assembly collection for the <see cref="TestTargetFramework"/>.
        /// </summary>
        /// <param name="targetFramework">The target framework.</param>
        /// <returns>The reference assembly collection and metadata references</returns>
        private static async Task<ImmutableArray<MetadataReference>> GetReferenceAssemblies(TestTargetFramework targetFramework = TestTargetFramework.Net)
        {
            throw new NotImplementedException();

            //// Compute the reference assemblies for the target framework.
            //if (targetFramework == TestTargetFramework.Net)
            //{
            //    return SourceGenerators.Tests.LiveReferencePack.GetMetadataReferences();
            //}
            //else
            //{
            //    var referenceAssembliesSdk = targetFramework switch
            //    {
            //        TestTargetFramework.Framework => ReferenceAssemblies.NetFramework.Net48.Default,
            //        TestTargetFramework.Standard => ReferenceAssemblies.NetStandard.NetStandard21,
            //        TestTargetFramework.Core => ReferenceAssemblies.NetCore.NetCoreApp31,
            //        TestTargetFramework.Net5 => ReferenceAssemblies.Net.Net50,
            //        TestTargetFramework.Net6 => ReferenceAssemblies.Net.Net60,
            //        _ => ReferenceAssemblies.Default
            //    };

            //    // Update the reference assemblies to include details from the NuGet.config.
            //    var referenceAssemblies = referenceAssembliesSdk
            //        .WithNuGetConfigFilePath(Path.Combine(Path.GetDirectoryName(typeof(TestUtils).Assembly.Location)!, "NuGet.config"));

            //    return await ResolveReferenceAssemblies(referenceAssemblies);
            //}
        }


        /// <summary>
        /// Run the supplied generators on the compilation.
        /// </summary>
        /// <param name="comp">Compilation target</param>
        /// <param name="diagnostics">Resulting diagnostics</param>
        /// <param name="generators">Source generator instances</param>
        /// <returns>The resulting compilation</returns>
        public static Compilation RunGenerators(Compilation comp, out ImmutableArray<Diagnostic> diagnostics, params IIncrementalGenerator[] generators)
        {
            CreateDriver(comp, null, generators).RunGeneratorsAndUpdateCompilation(comp, out var d, out diagnostics);
            return d;
        }

        /// <summary>
        /// Run the supplied generators on the compilation.
        /// </summary>
        /// <param name="comp">Compilation target</param>
        /// <param name="diagnostics">Resulting diagnostics</param>
        /// <param name="generators">Source generator instances</param>
        /// <returns>The resulting compilation</returns>
        public static Compilation RunGenerators(Compilation comp, AnalyzerConfigOptionsProvider options, out ImmutableArray<Diagnostic> diagnostics, params IIncrementalGenerator[] generators)
        {
            CreateDriver(comp, options, generators).RunGeneratorsAndUpdateCompilation(comp, out var d, out diagnostics);
            return d;
        }

        public static GeneratorDriver CreateDriver(Compilation c, AnalyzerConfigOptionsProvider? options, IIncrementalGenerator[] generators, GeneratorDriverOptions driverOptions = default)
            => CSharpGeneratorDriver.Create(
                ImmutableArray.Create(generators.Select(gen => gen.AsSourceGenerator()).ToArray()),
                parseOptions: (CSharpParseOptions)c.SyntaxTrees.First().Options,
                optionsProvider: options,
                driverOptions: driverOptions);

        private static async Task<ImmutableArray<MetadataReference>> ResolveReferenceAssemblies(ReferenceAssemblies referenceAssemblies)
        {
            try
            {
                ResolveRedirect.Instance.Start();
                return await referenceAssemblies.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);
            }
            finally
            {
                ResolveRedirect.Instance.Stop();
            }
        }

        private class ResolveRedirect
        {
            private const string EnvVarName = "NUGET_PACKAGES";

            private static readonly ResolveRedirect s_instance = new ResolveRedirect();
            public static ResolveRedirect Instance => s_instance;

            private int _count = 0;

            public void Start()
            {
                // Set the NuGet package cache location to a subdirectory such that we should always have access to it
                Environment.SetEnvironmentVariable(EnvVarName, Path.Combine(Path.GetDirectoryName(typeof(TestUtils).Assembly.Location)!, "packages"));
                Interlocked.Increment(ref _count);
            }

            public void Stop()
            {
                int count = Interlocked.Decrement(ref _count);
                if (count == 0)
                {
                   Environment.SetEnvironmentVariable(EnvVarName, null);
                }
            }
        }
    }
}
