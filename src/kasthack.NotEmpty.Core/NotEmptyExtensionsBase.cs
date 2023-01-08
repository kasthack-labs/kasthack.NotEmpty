namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public abstract class NotEmptyExtensionsBase
    {
        private static readonly IReadOnlySet<Type> KnownNumericTypes = new HashSet<Type>()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(char),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(BigInteger),
            typeof(Complex),
#if NET5_0_OR_GREATER
            typeof(Half),
            typeof(nint),
            typeof(nuint),
#endif
        };

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

        internal void NotEmptyInternal<T>(T? value, AssertContext context)
        {
            if (context.Options.MaxDepth != null && context.Options.MaxDepth.Value < context.CurrentDepth)
            {
                return;
            }

            var path = context.Path;
            string message = GetEmptyMessage(path);
            this.Assert(value is not null, message, path); // fast lane

            var valueType = typeof(T);
            var isKnownNumericType = KnownNumericTypes.Contains(valueType);

            var skipDueToBeingNumberInArrayWhenAllowedByOptions = context.ElementKind == ElementKind.ArrayElement
                && context.Options.AllowZerosInNumberArrays
                && isKnownNumericType;
            var skipDueToBeingBooleanPropertyWhenAllowedByOptions = context.Options.AllowFalseBooleanProperties && context.ElementKind == ElementKind.Property && value is bool;

            var skipDueToBeingEnumPropertyWhenAllowedByOptions = context.Options.AllowDefinedDefaultEnumValues && valueType.IsEnum && Enum.IsDefined(valueType, value!);

            if (!(
                skipDueToBeingBooleanPropertyWhenAllowedByOptions
                ||
                skipDueToBeingNumberInArrayWhenAllowedByOptions
                ||
                skipDueToBeingEnumPropertyWhenAllowedByOptions))
            {
                this.Assert(!EqualityComparer<T>.Default.Equals(default!, value!), message, path);
            }

            if (isKnownNumericType)
            {
                return;
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
#pragma warning disable SA1024 // Doesn't work with #if
                    :
#pragma warning restore SA1024
                    break;
                case string s:
                    this.Assert(
                        context.Options.AllowEmptyStrings
                            ? s != null
                            : !string.IsNullOrEmpty(s),
                        message,
                        path);
                    break;
                case System.Collections.IDictionary d:
                    var cnt = 0;
                    foreach (var key in d.Keys)
                    {
                        cnt++;
                        using (context.EnterPath($"[{key}]", ElementKind.DictionaryElement))
                        {
                            this.NotEmptyBoxed(d[key], context);
                        }
                    }

                    if (!context.Options.AllowEmptyCollections)
                    {
                        this.Assert(cnt != 0, message, path);
                    }

                    break;
                case System.Collections.IEnumerable e:
                    var index = 0;
                    foreach (var item in e)
                    {
                        using (context.EnterPath($"[{index++}]", ElementKind.ArrayElement))
                        {
                            this.NotEmptyBoxed(item, context);
                        }
                    }

                    if (!context.Options.AllowEmptyCollections)
                    {
                        this.Assert(index != 0, message, path);
                    }

                    break;
                default:
                    foreach (var pathValue in CachedPropertyExtractor<T>.GetPropertiesAndTupleFields(value!))
                    {
                        if (!pathValue.IsComputed || !context.Options.IgnoreComputedProperties)
                        {
                            using (context.EnterPath($".{pathValue.Path}", ElementKind.Property))
                            {
                                this.NotEmptyBoxed(pathValue.Value, context);
                            }
                        }
                    }

                    break;
            }
        }

        internal void NotEmptyBoxed(object? value, AssertContext context)
        {
            var path = context.Path;
            this.Assert(value is not null, GetEmptyMessage(path), path);
            CachedEmptyDelegate.GetDelegate(value!.GetType())(this, value, context);
        }

        protected abstract void Assert(bool value, string message, string path);

        private static string GetEmptyMessage(string? path) => $"{path} is empty";
    }
}