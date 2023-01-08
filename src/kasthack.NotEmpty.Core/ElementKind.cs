namespace kasthack.NotEmpty.Core
{
    internal enum ElementKind
    {
        /// <summary>
        /// No idea where we are.
        /// </summary>
        Unknown,

        /// <summary>
        /// Examinating root value.
        /// </summary>
        Root,

        /// <summary>
        /// Examinating an object property.
        /// </summary>
        Property,

        /// <summary>
        /// Examinating an array element.
        /// </summary>
        ArrayElement,

        /// <summary>
        /// Examinating a dictionary element.
        /// </summary>
        DictionaryElement,
    }
}