namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public abstract class NotEmptyExtensionsBase
    {
        [Obsolete($"Use {nameof(NotEmptyExtensionsBase.NotEmpty)}<T>(T, {nameof(AssertOptions)}) instead.")]
        public void NotEmpty<T>(T? value) => this.NotEmpty(value, null);

        public void NotEmpty<T>(T? value, AssertOptions? assertOptions)
        {
            assertOptions ??= AssertOptions.Default;
            if (assertOptions.MaxDepth.HasValue && assertOptions.MaxDepth < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(assertOptions)}.{nameof(assertOptions.MaxDepth)}", $"{nameof(assertOptions.MaxDepth)} must be greater than 0 / null for unlimited traversing.");
            }

            // workaround for boxed structs passed as objects
            this.NotEmptyBoxed(value, new AssertContext(assertOptions));
        }

        protected abstract void Assert(bool value, string message);

        internal void NotEmptyInternal<T>(T? value, AssertContext context)
        {
            if (context.Options.MaxDepth != null && context.Options.MaxDepth.Value < context.CurrentDepth)
            {
                return;
            }

            string message = GetEmptyMessage(context.Path);
            this.Assert(value is not null, message); // fast lane
            if (
                !(context.Options.AllowZerosInNumberArrays &&
                context.IsArrayElement &&
                value is byte or sbyte or short or ushort or char or int or uint or long or ulong or float or double or decimal or BigInteger
                #if NET5_0_OR_GREATER
                    or Half
                #endif
                ))
            {
                this.Assert(!EqualityComparer<T>.Default.Equals(default!, value!), message);
            }

            switch (value)
            {
                // ignore properties on builtin time structs as it's a reasonable thing to do
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
                        context.Options.AllowEmptyStrings
                            ? s != null
                            : !string.IsNullOrEmpty(s),
                        message);
                    break;
                case System.Collections.IDictionary d:
                    var cnt = 0;
                    foreach (var key in d.Keys)
                    {
                        cnt++;
                        using (context.EnterPath($"[{key}]", false))
                        {
                            this.NotEmptyBoxed(d[key], context);
                        }
                    }

                    if (!context.Options.AllowEmptyCollections)
                    {
                        this.Assert(cnt != 0, message);
                    }

                    break;
                case System.Collections.IEnumerable e:
                    var index = 0;
                    foreach (var item in e)
                    {
                        using (context.EnterPath(index++.ToString(), true))
                        {
                            this.NotEmptyBoxed(item, context);
                        }
                    }

                    if (!context.Options.AllowEmptyCollections)
                    {
                        this.Assert(index != 0, message);
                    }

                    break;
                default:
                    foreach (var pathValue in CachedPropertyExtractor<T>.GetProperties(value))
                    {
                        using (context.EnterPath(pathValue.Path, false))
                        {
                            this.NotEmptyBoxed(pathValue.Value, context);
                        }
                    }

                    break;
            }
        }

        internal void NotEmptyBoxed(object? value, AssertContext context)
        {
            this.Assert(value is not null, GetEmptyMessage(context.Path));
            CachedEmptyDelegate.GetDelegate(value!.GetType())(this, value, context);
        }

        private static string GetEmptyMessage(string? path) => $"value{path} is empty";
    }
}