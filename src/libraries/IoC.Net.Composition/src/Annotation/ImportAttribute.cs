namespace IoC.Net.Composition
{
    /// <summary>
    ///     Specifies that a property, field, or parameter imports a particular export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
                    AllowMultiple = false, Inherited = false)]
    public class ImportAttribute : System.ComponentModel.Composition.ImportAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportAttribute"/> class, importing the
        ///     export with the default contract name.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The default contract name is the result of calling
        ///         <see cref="AttributedModelServices.GetContractName(Type)"/> on the property, field,
        ///         or parameter type that this is marked with this attribute.
        ///     </para>
        ///     <para>
        ///         The contract name is compared using a case-sensitive, non-linguistic comparison
        ///         using <see cref="StringComparer.Ordinal"/>.
        ///     </para>
        /// </remarks>
        public ImportAttribute()
            : base((string?)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportAttribute"/> class, importing the
        ///     export with the contract name derived from the specified type.
        /// </summary>
        /// <param name="contractType">
        ///     A <see cref="Type"/> of which to derive the contract name of the export to import, or
        ///     <see langword="null"/> to use the default contract name.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         The contract name is the result of calling
        ///         <see cref="AttributedModelServices.GetContractName(Type)"/> on
        ///         <paramref name="contractType"/>.
        ///     </para>
        ///     <para>
        ///         The default contract name is the result of calling
        ///         <see cref="AttributedModelServices.GetContractName(Type)"/> on the property, field,
        ///         or parameter type that is marked with this attribute.
        ///     </para>
        ///     <para>
        ///         The contract name is compared using a case-sensitive, non-linguistic comparison
        ///         using <see cref="StringComparer.Ordinal"/>.
        ///     </para>
        /// </remarks>
        public ImportAttribute(Type? contractType)
            : base(null, contractType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportAttribute"/> class, importing the
        ///     export with the specified contract name.
        /// </summary>
        /// <param name="contractName">
        ///      A <see cref="string"/> containing the contract name of the export to import, or
        ///      <see langword="null"/> or an empty string ("") to use the default contract name.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         The default contract name is the result of calling
        ///         <see cref="AttributedModelServices.GetContractName(Type)"/> on the property, field,
        ///         or parameter type that is marked with this attribute.
        ///     </para>
        ///     <para>
        ///         The contract name is compared using a case-sensitive, non-linguistic comparison
        ///         using <see cref="StringComparer.Ordinal"/>.
        ///     </para>
        /// </remarks>
        public ImportAttribute(string? contractName)
            : base(contractName, null)
        {
        }

        public ImportAttribute(string? contractName, Type? contractType)
            : base(contractName, contractType)
        {
        }
    }
}
