namespace kasthack.NotEmpty.Core
{
    internal readonly struct PathValue
    {
        public PathValue(string path, object? value)
        {
            this.Path = path;
            this.Value = value;
        }

        public readonly string Path { get; }

        public readonly object? Value { get; }
    }
}