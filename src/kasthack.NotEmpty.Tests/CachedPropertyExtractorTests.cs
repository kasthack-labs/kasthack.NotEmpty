namespace kasthack.NotEmpty.Tests;

using System;
using System.Linq;

using global::Xunit;

using kasthack.NotEmpty.Core;
using kasthack.NotEmpty.Tests.SampleModels;

public class CachedPropertyExtractorTests
{
    [Fact]
    public void SampleRecordWorks() => AssertNumberOfProperties<SampleRecord>(1);

    [Fact]
    public void SampleRecordStructWorks() => AssertNumberOfProperties<SampleRecordStruct>(1);

    [Fact]
    public void SampleClassWithRwPropertyWorks() => AssertNumberOfProperties<SampleClassWithRwProperty>(1);

    [Fact]
    public void SampleClassWithInitOnlyPropertyWorks() => AssertNumberOfProperties<SampleClassWithInitOnlyProperty>(1);

    [Fact]
    public void SampleClassWithInitOnlyOldStylePropertyWorks() => AssertNumberOfProperties<SampleClassWithInitOnlyOldStyleProperty>(1);

    [Fact]
    public void SampleClassWithComputedPropertyWorks() => AssertNumberOfProperties<SampleClassWithComputedProperty>(0);

    [Fact]

    public void SampleClassWithSetOnlyPropertyWorks() => AssertNumberOfProperties<SampleClassWithSetOnlyProperty>(0);

    [Fact]
    public void AnonymousTypeWorks() => AssertNumberOfProperties(new { Value = 1 }, 1);

    [Fact]
    public void ValueTupleWorks() => AssertNumberOfProperties((value1: 1, value2: 1), 2);

    [Fact]
    public void SystemTupleWorks() => AssertNumberOfProperties(Tuple.Create(1), 1);

#pragma warning disable RCS1163, IDE0060 // Used for type inference.
    internal static void AssertNumberOfProperties<T>(T anonymousTypeForInference, int expected) => AssertNumberOfProperties<T>(expected);
#pragma warning restore IDE0060, RCS1163

    internal static void AssertNumberOfProperties<T>(int expected) => Assert.Equal(expected, CachedPropertyExtractor<T>.GetPropertyAccessors().Count(a => !a.Computed));
}
