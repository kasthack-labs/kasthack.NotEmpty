namespace kasthack.NotEmpty.Tests.SampleModels;

public struct InfiniteNestedStruct
{
    public InfiniteNestedStruct()
    {
    }

    public int Value { get; set; } = 1;

    public InfiniteNestedStruct Child => new() { Value = this.Value + 1 };
}