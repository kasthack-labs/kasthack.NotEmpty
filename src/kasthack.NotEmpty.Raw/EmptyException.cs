namespace kasthack.NotEmpty.Raw
{
    using System;

    public class EmptyException : Exception
    {
        public EmptyException(string message, string path)
            : base(message)
        {
            this.Path = path;
        }

        public string Path { get; }
    }
}