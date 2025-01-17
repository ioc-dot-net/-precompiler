﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace IoC.Net.Precompiler.Unit.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void SimpleGeneratorTest()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation("""
            namespace MyCode
            {
                public class Program
                {
                    public static void Main(string[] args)
                    {
                    }
                }
            }
            """);

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            PrecompilerSourceGenerator generator = new PrecompilerSourceGenerator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create((ISourceGenerator)generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators

            //// Or we can look at the results directly:
            //GeneratorDriverRunResult runResult = driver.GetRunResult();

            //// The runResult contains the combined results of all generators passed to the driver
            //Debug.Assert(runResult.GeneratedTrees.Length == 1);
            //Debug.Assert(runResult.Diagnostics.IsEmpty);

            //// Or you can access the individual results on a by-generator basis
            //GeneratorRunResult generatorResult = runResult.Results[0];
            //Debug.Assert(generatorResult.Generator == generator);
            //Debug.Assert(generatorResult.Diagnostics.IsEmpty);
            //Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            //Debug.Assert(generatorResult.Exception is null);
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    


    
        [Fact]
        public void RecordInExternalAssembly()
        {
            // Compile the referenced assembly first.
            Compilation compilation = CompilationHelper.CreateReferencedLibRecordCompilation();

            PrecompilerSourceGenerator generator = new PrecompilerSourceGenerator();

            Compilation newCompilation = CompilationHelper.RunGenerators(compilation, out ImmutableArray<Diagnostic> generatorDiags, generator);

            // Make sure compilation was successful.
            CheckCompilationDiagnosticsErrors(generatorDiags);
            CheckCompilationDiagnosticsErrors(newCompilation.GetDiagnostics());
        }

        private void CheckCompilationDiagnosticsErrors(ImmutableArray<Diagnostic> diagnostics)
        {
            Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        }

    }
}
