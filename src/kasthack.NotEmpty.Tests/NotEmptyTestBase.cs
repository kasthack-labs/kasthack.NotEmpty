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

        #region Object handling
        [Fact]
        public void NullThrows() => Assert.ThrowsAny<Exception>(() => this.Action(null));

        [Fact]
        public void NotNullEmptyObjectWorks() => this.Action(new object());

        [Fact]
        public void NotNullObjectWithNotNullPropertyWorks() => this.Action(new { Value = new object() });

        [Fact]
        public void NotNullObjectWithNullPropertyThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = (object?)null, }));

        [Fact]
        public void NestedObjectWithNullPropertyThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Property = new { Value = 0, } }));
        #endregion

        #region Numbers

        [Fact]
        public void NotDefaultPrimitiveWorks() => this.Action(new { Value = 1 });

        [Fact]
        public void BoxedNotDefaultPrimitiveWorks() => this.Action(new { Value = (object)1 });

        [Fact]
        public void NotNullNullableWorks() => this.Action(new { Value = (int?)1 });

        [Fact]
        public void BoxedNotNullNullableWorks() => this.Action(new { Value = (object)new Nullable<int>(1) });

        [Fact]
        public void DefaultPrimitiveThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = 0 }));

        [Fact]
        public void BoxedDefaultPrimitiveThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = (object)0 }));

        [Fact]
        public void BoxedDefaultPrimitiveThrowsAsRoot() => Assert.ThrowsAny<Exception>(() => this.Action(0));

        [Fact]
        public void NullNullableThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = new Nullable<int>(), }));

        [Fact]
        public void BoxedNullNullableThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = (object?)(new Nullable<int>())!, }));

        #endregion

        #region Enums

        // no false-positive
        [Fact]
        public void NotDefaultEnumWorks() => this.Action(new { Value = EnumWithDefaultValueDefined.No });

        // no false-negative
        [Fact]
        public void DefaultEnumThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = default(EnumWithDefaultValueDefined) }));

        [Fact]
        public void DefaultEnumDoesntThrowWhenAllowed() => this.Action(new { Value = default(EnumWithDefaultValueDefined) }, new AssertOptions { AllowDefinedDefaultEnumValues = true });

        [Fact]
        public void DefaultEnumThrowsWhenAllowedButNotDefined() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = default(EnumWithoutDefaultValue) }, new AssertOptions { AllowDefinedDefaultEnumValues = true }));

        enum EnumWithDefaultValueDefined
        {
            Yes,
            No,
        }

        enum EnumWithoutDefaultValue
        {
            Yes = 1,
            No = 2,
        }
        #endregion

        #region Strings
        [Fact]
        public void NotEmptyStringWorks() => this.Action("test string");

        [Fact]
        public void EmptyStringThrows() => Assert.ThrowsAny<Exception>(() => this.Action(string.Empty));

        [Fact]
        public void EmptyStringDoesntThrowWhenAllowed() => this.Action(string.Empty, new AssertOptions { AllowEmptyStrings = true, });

        #endregion

        #region Collections

        #region Emptinness

        [Fact]
        public void NotEmptyListWorks() => this.Action(new List<object> { new object() });

        [Fact]
        public void NotEmptyArrayWorks() => this.Action(new object[] { new object() });

        [Fact]
        public void EmptyArrayThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new object[] { }));

        [Fact]
        public void EmptyListThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new List<object>()));

        [Fact]
        public void EmptyArrayDoesntThrowWhenAllowed() => this.Action(new object[] { }, new AssertOptions { AllowEmptyCollections = true, });

        #endregion

        #region Dictionaries

        [Fact]
        public void EmptyDictionaryThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new Dictionary<string, string>()));

        [Fact]
        public void NotEmptyDictionaryWorks() => this.Action(new Dictionary<string, string> { { "key", "value" } });

        [Fact]
        public void NotEmptyDictionaryWithBoxedKeysWorks() => this.Action(new Dictionary<int, string> { { 1, "value" } });

        [Fact]
        public void DictionaryWithNullValuesThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new Dictionary<string, string>() { { "key", null! }, }));

        [Fact]
        public void EmptyDictionaryDoesntThrowWhenAllowed() => this.Action(new Dictionary<string, string>(), new AssertOptions { AllowEmptyCollections = true, });

        #endregion

        #region Default elements

        [Fact]
        public void ArrayWithDefaultPrimitiveThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new object[] { 1, 0 }));

        [Fact]
        public void ArrayWithDefaultPrimitiveDoesntThrowWhenAllowed() => this.Action(new object[] { 1, 0 }, new AssertOptions { AllowZerosInNumberArrays = true });

        [Fact]
        public void AllowZerosInNumberArraysWorksCorrectlyForChildren() => Assert.ThrowsAny<Exception>(() => this.Action(new[] { new { Value = 0 } }, new AssertOptions { AllowZerosInNumberArrays = true }));

        [Fact]
        public void ArrayWithNullThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new object?[] { new object(), null }));
        #endregion

        #endregion

        #region DateTime

        [Fact]
        public void EmptyDateTimeThrows() => Assert.ThrowsAny<Exception>(() => this.Action(default(DateTime)));

        [Fact]
        public void NotEmptyDateTimeWithSomeZeroesWorks() => this.Action(new DateTime(2000, 1, 1, 0, 0, 0));
        #endregion

        #region Depth

        [Fact(Skip = "OOM")]
        public void InfititeDepthThrowsWhenAllowed() => Assert.ThrowsAny<Exception>(() => this.Action(new InfiniteNestedStruct(), new AssertOptions { MaxDepth = null }));

        [Fact]
        public void InfiniteDepthDoesntThrowWithDefaultSettings() => this.Action(new InfiniteNestedStruct());

        [Fact]
        public void InvalidDepthThrows() => Assert.ThrowsAny<Exception>(() => this.Action(0, new AssertOptions { MaxDepth = -1 }));

        [Fact]
        public void DepthZeroWorks() => Assert.ThrowsAny<Exception>(() => this.Action(0, new AssertOptions { MaxDepth = 0 }));

        public struct InfiniteNestedStruct
        {
            public InfiniteNestedStruct()
            {
            }

            public int Value { get; set; } = 1;

            public InfiniteNestedStruct Child => new InfiniteNestedStruct { Value = this.Value + 1 };
        }
        #endregion

        #region Booleans

        [Fact]
        public void FalseThrows() => Assert.ThrowsAny<Exception>(() => this.Action(new { Value = false }));

        [Fact]
        public void TrueWorks() => this.Action(true);

        [Fact]
        public void FalseDoesntThrowWhenAllowed() => this.Action(new { Value = false }, new AssertOptions { AllowFalseBooleanProperties = true });

        [Fact]
        public void FalseThrowsWhenAllowedForDifferentKind() => Assert.ThrowsAny<Exception>(() => this.Action(false, new AssertOptions { AllowFalseBooleanProperties = true }));

        [Fact]
        public void FalseThrowsWhenAllowedForDifferentKindV2() => Assert.ThrowsAny<Exception>(() => this.Action(new[] { false }, new AssertOptions { AllowFalseBooleanProperties = true }));
        #endregion

        protected void Action(object? value, AssertOptions? options = null) => this.action(value, options);
    }

}