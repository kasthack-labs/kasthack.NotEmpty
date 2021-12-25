namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;

    public abstract class NotEmptyExtensionsBase
    {
        [Obsolete($"Use {nameof(NotEmptyExtensionsBase.NotEmpty)}<T>(T, {nameof(AssertOptions)}) instead.")]
        public void NotEmpty<T>(T? value) => this.NotEmpty(value, null);

        public void NotEmpty<T>(T? value, AssertOptions? assertOptions)
        {
            assertOptions ??= AssertOptions.Default;

            // workaround for boxed structs passed as objects
            if (value is not null && typeof(T) == typeof(object) && value.GetType() != typeof(object))
            {
                this.NotEmptyBoxed(value, assertOptions, null!);
            }

            this.NotEmptyInternal(value, assertOptions, null);
        }

        protected abstract void Assert(bool value, string message);

        internal void NotEmptyInternal<T>(T? value, AssertOptions assertOptions, string? path = null)
        {
            string message = GetEmptyMessage(path);
            this.Assert(value is not null, message); // fast lane
            this.Assert(!EqualityComparer<T>.Default.Equals(default!, value!), message);

            switch (value)
            {
                // TODO: add max-depth instead of doing this
                // infinite recursion workaround
                case DateTimeOffset _
                    or DateTime _
                    or TimeSpan _
#if NET6_0_OR_GREATER
                    or DateOnly _
                    or TimeOnly _
#endif
                    :
                    break;
                case string s:
                    this.Assert(
                        assertOptions.AllowEmptyStrings
                            ? s != null
                            : !string.IsNullOrEmpty(s),
                        message);
                    break;
                case System.Collections.IEnumerable e:
                    var index = 0;
                    foreach (var item in e)
                    {
                        this.NotEmptyBoxed(item, assertOptions, $"{path}[{index++}]");
                    }

                    if (!assertOptions.AllowEmptyCollections)
                    {
                        this.Assert(index != 0, message);
                    }

                    break;
                default:
                    foreach (var pathValue in CachedPropertyExtractor<T>.GetProperties(value))
                    {
                        this.NotEmptyBoxed(pathValue.Value, assertOptions, $"{path}.{pathValue.Path}");
                    }

                    break;
            }
        }

        internal void NotEmptyBoxed(object? value, AssertOptions assertOptions, string? path)
        {
            this.Assert(value is not null, GetEmptyMessage(path));
            CachedEmptyDelegate.GetDelegate(value!.GetType())(this, value, assertOptions, path);
        }

        private static string GetEmptyMessage(string? path) => $"value{path} is empty";
    }
}