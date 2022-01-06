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

        public ElementKind ElementKind { get; set; } = Core.ElementKind.Root;

        public AssertContext(AssertOptions options) => this.Options = options;

        public IDisposable EnterPath(string segment, ElementKind elementKind) => new PathContext(this, segment, elementKind);

        private struct PathContext : IDisposable
        {
            private readonly AssertContext context;
            private readonly ElementKind originalElementKind;
            private bool disposed = false;

            public PathContext(AssertContext context, string segment, ElementKind elementKind)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));
                this.originalElementKind = this.context.ElementKind;

                this.context.pathSegments.Push(segment);
                this.context.ElementKind = elementKind;
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.context.pathSegments.Pop();
                    this.context.ElementKind = this.originalElementKind;
                }
            }
        }
    }
}