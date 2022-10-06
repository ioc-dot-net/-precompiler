namespace IoC.Net.Precompiler
{
    public partial class PrecompilerSourceGenerator : ISourceGenerator
    {
        /// <summary>
        /// Registers a syntax resolver to receive compilation units.
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(static () => new SyntaxContextReceiver());
        }

        /// <summary>
        /// Generates source code
        /// </summary>
        /// <param name="executionContext"></param>
        public void Execute(GeneratorExecutionContext executionContext)
        {
#if LAUNCH_DEBUGGER
            if (!Diagnostics.Debugger.IsAttached)
            {
                Diagnostics.Debugger.Launch();
            }
#endif
            if (executionContext.SyntaxContextReceiver is not SyntaxContextReceiver receiver || 
                receiver.ClassDeclarationSyntaxList == null)
            {
                // nothing to do yet
                return;
            }

            //JsonSourceGenerationContext context = new JsonSourceGenerationContext(executionContext);
            //Parser parser = new(executionContext.Compilation, context);
            //SourceGenerationSpec? spec = parser.GetGenerationSpec(receiver.ClassDeclarationSyntaxList, executionContext.CancellationToken);
            //if (spec != null)
            //{
            //    _rootTypes = spec.ContextGenerationSpecList[0].RootSerializableTypes;

            //    Emitter emitter = new(context, spec);
            //    emitter.Emit();
            //}
        }
    }
}
