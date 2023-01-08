namespace kasthack.NotEmpty.Raw
{
    using kasthack.NotEmpty.Core;

    /// <summary>
    /// .NotEmpty&lt;T&gt;() extensions.
    /// </summary>
    public static class NotEmptyExtensions
    {
        private static readonly NotEmptyExtensionsBaseRaw Instance = new();

        /// <summary>
        /// Tests objects for emptinness(being null, default(T), empty collection or string) recursively.
        /// </summary>
        /// <param name="value">Value to test for emptinness.</param>
        /// <param name="assertOptions">Test options.</param>
        /// <typeparam name="T">Type of value(inferred by the compiler).</typeparam>
        public static void NotEmpty<T>(this T? value, AssertOptions? assertOptions = null) => Instance.NotEmpty(value, assertOptions);

        private sealed class NotEmptyExtensionsBaseRaw : NotEmptyExtensionsBase
        {
            protected override void Assert(bool value, string message, string path)
            {
                if (!value)
                {
                    throw new EmptyException(message, path);
                }
            }
        }
    }
}