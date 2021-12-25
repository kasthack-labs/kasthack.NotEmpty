namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Reflection;

    // Returns all properties as an array of KV pairs
    internal static class CachedPropertyExtractor<T>
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
}