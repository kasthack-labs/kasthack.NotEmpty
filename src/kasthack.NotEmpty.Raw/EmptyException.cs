namespace kasthack.NotEmpty.Raw
{
    using System;

#pragma warning disable RCS1194 // Implement exception constructors.
    public sealed class EmptyException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
    {
        internal EmptyException(string message, string path)
            : base(message)
        {
            this.Path = path;
        }

        public string Path { get; }
    }
}