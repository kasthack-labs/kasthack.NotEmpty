namespace kasthack.NotEmpty.Core
{
    public class AssertOptions
    {
        /// <summary>
        /// Allow zeros in number arrays. Useful when you have binary data as a byte array.
        /// </summary>
        public bool AllowZerosInNumberArrays { get; set; } = false;

        /// <summary>
        /// Maximum assert depth. Useful for preventing stack overflows for objects with generated properties / complex graphs.
        /// </summary>
        public int? MaxDepth { get; set; } = 100;

        /// <summary>
        /// Allows empty strings but not nulls.
        /// </summary>
        public bool AllowEmptyStrings { get; set; } = false;

        /// <summary>
        /// Allows empty strings but not nulls.
        /// </summary>
        public bool AllowEmptyCollections { get; set; } = false;

        /// <summary>
        /// Allows bool properties to be false.
        /// </summary>
        public bool AllowFalseBooleanProperties { get; set; } = false;

        /// <summary>
        /// Allows enum values to have default values if there's a defined option for that.
        /// </summary>
        public bool AllowDefinedDefaultEnumValues { get; set; } = false;
        internal static AssertOptions Default { get; } = new();
    }
}