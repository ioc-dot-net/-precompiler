namespace IoC.Net.Precompiler
{
    public partial class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public List<ClassDeclarationSyntax>? ClassDeclarationSyntaxList { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (IsSyntaxTargetForGeneration(context.Node))
            {
                ClassDeclarationSyntax? classSyntax = GetSemanticTargetForGeneration(context, CancellationToken.None);
                if (classSyntax != null)
                {
                    (ClassDeclarationSyntaxList ??= new List<ClassDeclarationSyntax>()).Add(classSyntax);
                }
            }
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0, BaseList.Types.Count: > 0 };

        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    IMethodSymbol? attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax, CancellationToken.None).Symbol as IMethodSymbol;
                    if (attributeSymbol == null)
                    {
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    //if (fullName == Parser.JsonSerializableAttributeFullName)
                    //{
                    //    return classDeclarationSyntax;
                    //}
                }

            }

            return null;
        }
    }
}
