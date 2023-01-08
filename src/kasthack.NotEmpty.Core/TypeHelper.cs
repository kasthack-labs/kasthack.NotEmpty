namespace kasthack.NotEmpty.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
internal static class TypeHelper
{
    private static readonly IReadOnlySet<Type> ValueTupleTypes = new HashSet<Type>()
    {
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>),
        typeof(ValueTuple<,,,,,,,>),
        typeof(ValueTuple<,,,,,,,>),
    };

    private static readonly IReadOnlySet<Type> TupleTypes = new HashSet<Type>()
    {
        typeof(Tuple<>),
        typeof(Tuple<,>),
        typeof(Tuple<,,>),
        typeof(Tuple<,,,>),
        typeof(Tuple<,,,,>),
        typeof(Tuple<,,,,,>),
        typeof(Tuple<,,,,,,>),
        typeof(Tuple<,,,,,,,>),
        typeof(Tuple<,,,,,,,>),
    };

    public static bool IsTuple(this Type type) => type.IsGenericType && TupleTypes.Contains(type.GetGenericTypeDefinition());

    public static bool IsValueTuple(this Type type) => type.IsGenericType && ValueTupleTypes.Contains(type.GetGenericTypeDefinition());

    // https://stackoverflow.com/a/11472757/17594255
    public static bool IsAnonymousType(this Type type)
    {
        if (type.IsGenericType)
        {
            var d = type.GetGenericTypeDefinition();
            if (d.IsClass && d.IsSealed && d.Attributes.HasFlag(TypeAttributes.NotPublic))
            {
                var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                if (attributes?.Length > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}