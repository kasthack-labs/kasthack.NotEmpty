namespace kasthack.NotEmpty.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

// Returns all properties as an array of KV pairs
internal static class CachedPropertyExtractor<T>
{
    private static IReadOnlyList<(string Name, bool Computed, Func<T, object?> Getter)> Properties { get; } = GetPropertyAccessors();

    public static PathValue[] GetPropertiesAndTupleFields(T value)
    {
        if (Properties.Count == 0)
        {
            return Array.Empty<PathValue>();
        }

        var props = new PathValue[Properties.Count];
        for (int i = 0; i < props.Length; i++)
        {
            var (name, computed, getter) = Properties[i];
            props[i] = new PathValue(name, computed, getter(value!));
        }

        return props;
    }

    internal static IReadOnlyList<(string Name, bool Computed, Func<T, object?> Getter)> GetPropertyAccessors()
    {
        // https://stackoverflow.com/questions/2210309/how-to-find-out-if-a-property-is-an-auto-implemented-property-with-reflection
        var type = typeof(T);

        var fieldnames = type
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(a => a.Name)
            .ToHashSet();
        var allProperties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var propertyAccessors = allProperties
            .Where(property => property.CanRead && property.GetMethod!.IsPublic)
            .Select<PropertyInfo, (string Name, bool Computed, Func<Expression, MemberExpression> ExpressionBuilder)>(
                propertyInfo => (propertyInfo.Name, IsComputed(propertyInfo), instance => Expression.Property(instance, propertyInfo)))
            .ToList();

        var fieldAccessors = type.IsValueTuple()
            ? type
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select<FieldInfo, (string Name, bool Computed, Func<Expression, MemberExpression> ExpressionBuilder)>(
                    fieldInfo => (fieldInfo.Name, false, instance => Expression.Field(instance, fieldInfo)))
                .ToArray()
            : Array.Empty<(string Name, bool Computed, Func<Expression, MemberExpression> ExpressionBuilder)>();

        return propertyAccessors
            .Concat(fieldAccessors)
            .Select(accessorPair =>
            {
                var instance = Expression.Parameter(typeof(T), "instance");
                var memberAccessor = accessorPair.ExpressionBuilder(instance);
                var convert = Expression.Convert(memberAccessor, typeof(object));
                var lambda = Expression.Lambda<Func<T, object?>>(convert, instance);

                return (accessorPair.Name, accessorPair.Computed, lambda.Compile());
            }).ToArray();

        bool IsComputed(PropertyInfo property)
        {
            // has a setter / init
            if (property.CanWrite)
            {
                return false;
            }

            // get-only auto-property
            var isGetOnlyAutoProperty =
                property.GetMethod!.CustomAttributes.Any(a => a.AttributeType == typeof(CompilerGeneratedAttribute))
                && fieldnames.Contains($"<{property.Name}>k__BackingField");

            if (isGetOnlyAutoProperty)
            {
                return false;
            }

            // init-only property in anonymous type
            if (type.IsAnonymousType())
            {
                return false;
            }

            // built-in tuples
            if (type.IsTuple())
            {
                return false;
            }

            return true;
        }
    }
}
