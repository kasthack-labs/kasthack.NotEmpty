namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class NotEmptyExtensionsBase
    {
        public void NotEmpty<T>(T? value) => this.NotEmptyInternal(value);

        protected abstract void Assert(bool value, string message);

        private void NotEmptyInternal<T>(T? value, string? path = null)
        {
            string message = $"value{path} is empty";
            this.Assert(!EqualityComparer<T>.Default.Equals(default!, value!), message);
            switch (value)
            {
                case string s:
                    this.Assert(!string.IsNullOrEmpty(s), message);
                    break;
                case System.Collections.IEnumerable e:
                    var index = 0;
                    foreach (var item in e)
                    {
                        this.NotEmptyBox(item, $"{path}[{index++}]");
                    }

                    this.Assert(index != 0, message);
                    break;
                default:
                    foreach (var pathValue in CachedPropertyExtractor<T>.GetProperties(value))
                    {
                        this.NotEmptyBox(pathValue.Value, $"{path}.{pathValue.Path}");
                    }

                    break;
            }
        }

        private void NotEmptyBox(object? value, string path)
        {
            this.Assert(value is not null, $"value{path} is empty");
            CachedEmptyDelegate.GetDelegate(value!.GetType())(value, path);
        }

        private static class CachedEmptyDelegate
        {
            private static readonly MethodInfo NotEmptyMethod = typeof(NotEmptyExtensionsBase).GetMethod(nameof(NotEmptyExtensionsBase.NotEmptyInternal), BindingFlags.NonPublic | BindingFlags.Static)!.GetGenericMethodDefinition();
            private static readonly Dictionary<Type, Action<object?, string>> Delegates = new();

            public static Action<object?, string> GetDelegate(Type type)
            {
                if (!Delegates.TryGetValue(type, out var result))
                {
                    lock (Delegates)
                    {
                        if (!Delegates.TryGetValue(type, out result))
                        {
                            var valueParam = Expression.Parameter(typeof(object));
                            var pathParam = Expression.Parameter(typeof(string));
                            result = (Action<object?, string>)Expression
                                .Lambda(
                                    Expression.Call(
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

        private class PathValue
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