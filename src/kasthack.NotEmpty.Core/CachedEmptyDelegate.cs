namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    // Creates NotEmptyInternal<T> wrapper:
    // (object value, AssertOptions options, string path) => this.NotEmptyInternal<ACTUAL_TYPE_OF_VALUE>((ACTUAL_TYPE_OF_VALUE)value, options, path)
    internal static class CachedEmptyDelegate
    {
        private static readonly MethodInfo NotEmptyMethod = typeof(NotEmptyExtensionsBase)
            .GetMethod(nameof(NotEmptyExtensionsBase.NotEmptyInternal), BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetGenericMethodDefinition();

        private static readonly Dictionary<Type, Action<NotEmptyExtensionsBase, object?, AssertContext>> Delegates = new();

        public static Action<NotEmptyExtensionsBase, object?, AssertContext> GetDelegate(Type type)
        {
            if (!Delegates.TryGetValue(type, out var result))
            {
                lock (Delegates)
                {
                    if (!Delegates.TryGetValue(type, out result))
                    {
                        var thisParam = Expression.Parameter(typeof(NotEmptyExtensionsBase));
                        var valueParam = Expression.Parameter(typeof(object));
                        var contextParam = Expression.Parameter(typeof(AssertContext));
                        var parameters = new[]
                        {
                                thisParam,
                                valueParam,
                                contextParam,
                        };
                        result = (Action<NotEmptyExtensionsBase, object?, AssertContext>)Expression
                            .Lambda(
                                Expression.Call(
                                    thisParam,
                                    NotEmptyMethod.MakeGenericMethod(type),
                                    arguments: new Expression[]
                                    {
                                        Expression.Convert(
                                            valueParam,
                                            type),
                                        contextParam,
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
}