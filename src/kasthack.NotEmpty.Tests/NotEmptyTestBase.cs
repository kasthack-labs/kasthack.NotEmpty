namespace kasthack.NotEmpty.Tests
{
    using System;
    using System.Collections.Generic;

    using global::Xunit;

    public abstract class NotEmptyTestBase
    {
        private readonly Action<object?> action;

        public NotEmptyTestBase(Action<object?> action) => this.action = action;

        [Fact]
        public void NullThrows() => Assert.ThrowsAny<Exception>(() => this.action(null));

        [Fact]
        public void ObjectWorks() => this.action(new object());

        [Fact]
        public void ObjectWithPropsWorks() => this.action(new { Value = 1 });

        [Fact]
        public void ObjectWithDefaultThrows() => Assert.ThrowsAny<Exception>(() => this.action(new { Value = 0 }));

        [Fact]
        public void NestedObjectWithDefaultThrows() => Assert.ThrowsAny<Exception>(() => this.action(new { Property = new { Value = 0, } }));

        [Fact]
        public void PrimitiveWorks() => this.action(1);

        [Fact]
        public void EmptyStringThrows() => Assert.ThrowsAny<Exception>(() => this.action(string.Empty));

        [Fact]
        public void DefaultThrows() => Assert.ThrowsAny<Exception>(() => this.action(0));

        [Fact]
        public void EmptyArrayThrows() => Assert.ThrowsAny<Exception>(() => this.action(new object[] { }));

        [Fact]
        public void EmptyListThrows() => Assert.ThrowsAny<Exception>(() => this.action(new List<object>()));

        [Fact]
        public void ArrayWithDefaultThrows() => Assert.ThrowsAny<Exception>(() => this.action(new object[] { 1, 0 }));

        [Fact]
        public void ArrayWithNullThrows() => Assert.ThrowsAny<Exception>(() => this.action(new object?[] { null, new object() }));

        [Fact]
        public void ListWorks() => this.action(new List<object> { new object() });

        [Fact]
        public void ArrayWorks() => this.action(new object[] { new object() });

        [Fact]
        public void EmptyDateTimeThrows() => Assert.ThrowsAny<Exception>(() => this.action(default(DateTime)));

        [Fact]
        public void KnownTypeWithInfiniteRecursionDoesntThrow() => this.action(new DateTime(2000, 1, 1, 0, 0, 0));
    }
}