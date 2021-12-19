namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class NotEmptyExtensionsBase
    {
        public void NotEmpty<T>(T? value)
        {
            // workaround for boxed structs passed as objects
            if (value is not null && typeof(T) == typeof(object) && value.GetType() != typeof(object))
            {
                this.NotEmptyBoxed(value, null!);
            }

            this.NotEmptyInternal(value);
        }

        protected abstract void Assert(bool value, string message);

        private static string GetEmptyMessage(string? path) => $"value{path} is empty";

        private void NotEmptyInternal<T>(T? value, string? path = null)
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
                    this.Assert(!string.IsNullOrEmpty(s), message);
                    break;
                case System.Collections.IEnumerable e:
                    var index = 0;
                    foreach (var item in e)
                    {
                        this.NotEmptyBoxed(item, $"{path}[{index++}]");
                    }

                    this.Assert(index != 0, message);
                    break;
                default:
                    foreach (var pathValue in CachedPropertyExtractor<T>.GetProperties(value))
                    {
                        this.NotEmptyBoxed(pathValue.Value, $"{path}.{pathValue.Path}");
                    }

                    break;
            }
        }

        private void NotEmptyBoxed(object? value, string? path)
        {
            this.Assert(value is not null, GetEmptyMessage(path));
            CachedEmptyDelegate.GetDelegate(this, value!.GetType())(value, path);
        }

        // Creates NotEmptyInternal<T> wrapper:
        // (object value, string path) => this.NotEmptyInternal<ACTUAL_TYPE_OF_VALUE>((ACTUAL_TYPE_OF_VALUE)value, path)
        private static class CachedEmptyDelegate
        {
            private static readonly MethodInfo NotEmptyMethod = typeof(NotEmptyExtensionsBase)
                .GetMethod(nameof(NotEmptyExtensionsBase.NotEmptyInternal), BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetGenericMethodDefinition();

            private static readonly Dictionary<Type, Action<object?, string?>> Delegates = new();

            public static Action<object?, string?> GetDelegate(NotEmptyExtensionsBase @this, Type type)
            {
                if (!Delegates.TryGetValue(type, out var result))
                {
                    lock (Delegates)
                    {
                        if (!Delegates.TryGetValue(type, out result))
                        {
                            var valueParam = Expression.Parameter(typeof(object));
                            var pathParam = Expression.Parameter(typeof(string));
                            result = (Action<object?, string?>)Expression
                                .Lambda(
                                    Expression.Call(
                                        Expression.Constant(@this),
                                        NotEmptyMethod.MakeGenericMethod(type),
                                        Expression.Convert(
                                            valueParam,
                                            type),
                                        pathParam),
                                    valueParam,
                                    pathParam)
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
}