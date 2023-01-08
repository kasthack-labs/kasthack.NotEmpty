namespace kasthack.NotEmpty.Core
{
    internal readonly record struct PathValue(string Path, bool IsComputed, object? Value);
}