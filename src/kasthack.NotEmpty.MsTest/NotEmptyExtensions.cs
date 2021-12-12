namespace kasthack.NotEmpty.MsTest
{
    using kasthack.NotEmpty.Core;

    /// <summary>
    /// MsTest .NotEmpty&lt;T&gt;() extensions.
    /// </summary>
    public static class NotEmptyExtensions
    {
        private static readonly NotEmptyExtensionsBaseXunit Instance = new();

        /// <summary>
        /// Tests objects for emptinness(being null, default(T), empty collection or string) recursively.
        /// </summary>
        /// <param name="value">Value to test for emptinness.</param>
        /// <typeparam name="T">Type of value(inferred by the compiler).</typeparam>
        public static void NotEmpty<T>(this T? value) => Instance.NotEmpty(value);

        private class NotEmptyExtensionsBaseXunit : NotEmptyExtensionsBase
        {
            protected override void Assert(bool value, string message) => global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(value, message);
        }
    }
}