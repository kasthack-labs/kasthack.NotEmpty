namespace kasthack.NotEmpty.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Dispose doesn't do what you migth think.
    /// </summary>
    internal class AssertContext
    {
        private readonly Stack<string> pathSegments = new();

        public AssertOptions Options { get; }

        public int CurrentDepth => this.pathSegments.Count;

        public string Path => "(value)" + string.Concat(this.pathSegments.Reverse());

        public bool IsArrayElement { get; set; } = false;

        public AssertContext(AssertOptions options) => this.Options = options;

        public IDisposable EnterPath(string segment, bool isArray) => new PathContext(this, segment, isArray);

        private struct PathContext : IDisposable
        {
            private readonly AssertContext context;
            private readonly bool originalIsArrayElement;
            private bool disposed = false;

            public PathContext(AssertContext context, string segment, bool isArray)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));
                this.originalIsArrayElement = this.context.IsArrayElement;

                this.context.pathSegments.Push(segment);
                this.context.IsArrayElement = isArray;
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.context.pathSegments.Pop();
                    this.context.IsArrayElement = this.originalIsArrayElement;
                }
            }
        }
    }
}