# kasthack.NotEmpty

[![Github All Releases](https://img.shields.io/github/downloads/kasthack-labs/kasthack.NotEmpty/total.svg)](https://github.com/kasthack-labs/kasthack.NotEmpty/releases/latest)
[![GitHub release](https://img.shields.io/github/release/kasthack-labs/kasthack.NotEmpty.svg)](https://github.com/kasthack-labs/kasthack.NotEmpty/releases/latest)
[![license](https://img.shields.io/github/license/kasthack-labs/kasthack.NotEmpty.svg)](LICENSE)
[![.NET Status](https://github.com/kasthack-labs/kasthack.NotEmpty/workflows/.NET/badge.svg)](https://github.com/kasthack-labs/kasthack.NotEmpty/actions?query=workflow%3A.NET)
[![CodeQL](https://github.com/kasthack-labs/kasthack.NotEmpty/workflows/CodeQL/badge.svg)](https://github.com/kasthack-labs/kasthack.NotEmpty/actions?query=workflow%3ACodeQL)
[![Patreon pledges](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dkasthack%26type%3Dpledges&style=flat)](https://patreon.com/kasthack)
[![Patreon patrons](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dkasthack%26type%3Dpatrons&style=flat)](https://patreon.com/kasthack)

## What?

kasthack.Empty is a library for recursively checking objects for emptinness(being null, default value, an empty collection or a string). It saves you from writing boilerplate in tests for deserializers / parsers / API clients.

## Why does this exist?

Manually checking properties for emptinness leaves an opportunity to miss something and makes the developer to write boilerplate.

## Usage

1. Install the appropriate package

* [Nunit](https://www.nuget.org/packages/kasthack.NotEmpty.Nunit/)
* [Xunit](https://www.nuget.org/packages/kasthack.NotEmpty.Xunit/)
* [MsTest](https://www.nuget.org/packages/kasthack.NotEmpty.MsTest/)
* [&lt;no frameworks&gt;](https://www.nuget.org/packages/kasthack.NotEmpty.Raw/)

2. Check your objects / their properties for emptinness. Look at the <a href="src/kasthack.NotEmpty.Tests/NotEmptyTestBase.cs">tests</a> for more details.

````csharp
using kasthack.NotEmpty.Xunit;  // replace the namespace to match your test framework

public class MyAmazingTest
{
    [Fact]
    public void MyThingWorks()
    {
        var targetObject = MyClass.GetResult();

        targetObject.NotEmpty();

        //<actual asserts>
    }

    [Fact]
    public void TestOptions()
    {
        // won't throw
        new {
            PropertyThanLegitimatelyCanBeAnEmptyStringButNotNull = "",
        }.NotEmpty(new AssertOptions {
            AllowEmptyStrings = true,
        });

        //won't throw
        new {
            PropertyThanLegitimatelyCanBeAnEmptyCollectionButNotNull = new int[]{},
        }.NotEmpty(new AssertOptions {
            AllowEmptyCollections = true,
        });

        // won't throw
        new {
            FileContentThatObviouslyContainsSomeNullBytes = new byte[]{ 0 }
        }.NotEmpty(new AssertOptions {
            AllowZerosInNumberArrays = true
        });

        // won't throw BUT will stop at 200 iterations
        // default MaxDepth is 100
        new {
            DeeplyNestedObject = new InfiniteNestedStruct()
        }.NotEmpty(new AssertOptions {
            MaxDepth = 200
        });
    }

    public struct InfiniteNestedStruct
    {
        public int Value { get; set; } = 1;

        public InfiniteNestedStruct Child => new InfiniteNestedStruct { Value = this.Value + 1 };
    }
}
````
