namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

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

        private static string GetEmptyMessage(string? path) => $"value{path} is empty";

        private void NotEmptyInternal<T>(T? value, AssertOptions assertOptions, string? path = null)
        {
            string message = GetEmptyMessage(path);
            this.Assert(value is not null, message); // fast lane
            this.Assert(!EqualityComparer<T>.Default.Equals(default!, value!), message);

            switch (value)
            {
                // TODO: add max-depth instead of doing this
                // infinite recursion workaround
                case DateTimeOffset _:
                case DateTime _:
                case TimeSpan _:
#if NET6_0_OR_GREATER
                case DateOnly _:
                case TimeOnly _:
#endif
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

                    this.Assert(index != 0, message);
                    break;
                default:
                    foreach (var pathValue in CachedPropertyExtractor<T>.GetProperties(value))
                    {
                        this.NotEmptyBoxed(pathValue.Value, assertOptions, $"{path}.{pathValue.Path}");
                    }

                    break;
            }
        }

        private void NotEmptyBoxed(object? value, AssertOptions assertOptions, string? path)
        {
            this.Assert(value is not null, GetEmptyMessage(path));
            CachedEmptyDelegate.GetDelegate(value!.GetType())(this, value, assertOptions, path);
        }

        // Creates NotEmptyInternal<T> wrapper:
        // (object value, AssertOptions options, string path) => this.NotEmptyInternal<ACTUAL_TYPE_OF_VALUE>((ACTUAL_TYPE_OF_VALUE)value, options, path)
        private static class CachedEmptyDelegate
        {
            private static readonly MethodInfo NotEmptyMethod = typeof(NotEmptyExtensionsBase)
                .GetMethod(nameof(NotEmptyExtensionsBase.NotEmptyInternal), BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetGenericMethodDefinition();

            private static readonly Dictionary<Type, Action<NotEmptyExtensionsBase, object?, AssertOptions, string?>> Delegates = new();

            public static Action<NotEmptyExtensionsBase, object?, AssertOptions, string?> GetDelegate(Type type)
            {
                if (!Delegates.TryGetValue(type, out var result))
                {
                    lock (Delegates)
                    {
                        if (!Delegates.TryGetValue(type, out result))
                        {
                            var thisParam = Expression.Parameter(typeof(NotEmptyExtensionsBase));
                            var valueParam = Expression.Parameter(typeof(object));
                            var optionsParam = Expression.Parameter(typeof(AssertOptions));
                            var pathParam = Expression.Parameter(typeof(string));
                            var parameters = new[]
                            {
                                thisParam,
                                valueParam,
                                optionsParam,
                                pathParam,
                            };
                            result = (Action<NotEmptyExtensionsBase, object?, AssertOptions, string?>)Expression
                                .Lambda(
                                    Expression.Call(
                                        thisParam,
                                        NotEmptyMethod.MakeGenericMethod(type),
                                        arguments: new Expression[]
                                        {
                                            Expression.Convert(
                                                valueParam,
                                                type),
                                            optionsParam,
                                            pathParam,
                                        }),
                                    parameters)
                                .Compile();
                            Delegates[type] = result;
                        }
                    }
                }

                return result;
            }
        }

        // Returns all properties as an array of KV pairs
        private static class CachedPropertyExtractor<T>
        {
            private static readonly PropertyInfo[] Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            public static PathValue[] GetProperties(T? value)
            {
                if (Properties.Length == 0)
                {
                    return Array.Empty<PathValue>();
                }

                var props = new PathValue[Properties.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    props[i] = new PathValue(Properties[i].Name, Properties[i].GetValue(value));
                }

                return props;
            }
        }

        private struct PathValue
        {
            public PathValue(string path, object? value)
            {
                this.Path = path;
                this.Value = value;
            }

            public string Path { get; }

            public object? Value { get; }
        }
    }

    public class AssertOptions
    {
        internal static AssertOptions Default { get; } = new();

        /*
        ///// <summary>
        ///// Maximum assert depth. Useful for preventing stack overflows for objects with generated properties / complex graphs.
        ///// </summary>
        //public int? MaxDepth { get; set; } = 100;

        ///// <summary>
        ///// Allow zeros in number arrays. Useful when you have binary data as a byte array.
        ///// </summary>
        //public bool AllowZerosInNumberArrays { get; set; } = false;
        */

        /// <summary>
        /// Allows empty strings but not nulls.
        /// </summary>
        public bool AllowEmptyStrings { get; set; } = false;
    }
}