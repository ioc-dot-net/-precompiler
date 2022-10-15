
namespace IoC.Net.Precompiler
{
    public partial class PrecompilerSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            //    .ForAttributeWithMetadataName(Parser.JsonSerializableAttributeFullName,
            //                                  (node, _) => node is ClassDeclarationSyntax,
            //                                  (context, _) => (ClassDeclarationSyntax)context.TargetNode);

            //IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses =
            //    context.CompilationProvider.Combine(classDeclarations.Collect());

            //context.RegisterSourceOutput(compilationAndClasses, (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> contextClasses, SourceProductionContext sourceProductionContext)
        {
#if LAUNCH_DEBUGGER
            if (!Diagnostics.Debugger.IsAttached)
            {
                Diagnostics.Debugger.Launch();
            }
#endif
            if (contextClasses.IsDefaultOrEmpty)
            {
                return;
            }

            //JsonSourceGenerationContext context = new JsonSourceGenerationContext(sourceProductionContext);
            //Parser parser = new(compilation, context);
            //SourceGenerationSpec? spec = parser.GetGenerationSpec(contextClasses, sourceProductionContext.CancellationToken);
            //if (spec != null)
            //{
            //    _rootTypes = spec.ContextGenerationSpecList[0].RootSerializableTypes;

            //    Emitter emitter = new(context, spec);
            //    emitter.Emit();
            //}
        }
    }
}
