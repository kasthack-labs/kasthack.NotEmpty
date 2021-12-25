namespace kasthack.NotEmpty.Tests
{
    using System;
    using System.Collections.Generic;

    using global::Xunit;

    using kasthack.NotEmpty.Core;

    public abstract class NotEmptyTestBase
    {
        private readonly Action<object?, AssertOptions?> action;

        public NotEmptyTestBase(Action<object?, AssertOptions?> action) => this.action = action;

        [Fact]
        public void NullThrows() => Assert.ThrowsAny<Exception>(() => this.Action(null));

        [Fact]
        public void ObjectWorks() => this.Action(new object());

        [Fact]
        public void ObjectWithPropsWorks() => this.Action(new { Value = 1 });

        [Fact]
        public void ObjectWithDefaultThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = 0 }));

        [Fact]
        public void NestedObjectWithDefaultThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Property = new { Value = 0, } }));

        [Fact]
        public void PrimitiveWorks() => this.Action(1);

        [Fact]
        public void EmptyStringThrows() => Assert.ThrowsAny<Exception>(() => this.Action(string.Empty));

        [Fact]
        public void DefaultThrows() => Assert.ThrowsAny<Exception>(() => this.Action(0));

        [Fact]
        public void EmptyArrayThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new object[] { }));

        [Fact]
        public void EmptyListThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new List<object>()));

        [Fact]
        public void ArrayWithDefaultThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new object[] { 1, 0 }));

        [Fact]
        public void ArrayWithNullThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new object?[] { null, new object() }));

        [Fact]
        public void ListWorks() => this.Action(new List<object> { new object() });

        [Fact]
        public void ArrayWorks() => this.Action(new object[] { new object() });

        [Fact]
        public void EmptyDateTimeThrows() => Assert.ThrowsAny<Exception>(() => this.Action(default(DateTime)));

        [Fact]
        public void KnownTypeWithInfiniteRecursionDoesntThrow() => this.Action(new DateTime(2000, 1, 1, 0, 0, 0));

        [Fact]
        public void AllowsEmptyStringsWithConfiguredOption() => this.Action("", new AssertOptions { AllowEmptyStrings = true, });

        private void Action(object? value, AssertOptions? options = null) => this.action(value, options);
    }
}