// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Logging.Abstractions;
using ReferenceResolver;

namespace UnitTest;

public class ReferenceTest
{
    [Fact]
    public void FileReferenceTypeTest()
    {
        IReference reference = new FileReference("ReferenceResolver.dll");
        Assert.Equal(ReferenceType.LocalFile, reference.ReferenceType);
        Assert.Equal($"file: {reference.Reference}", reference.ReferenceWithSchema);
    }

    [Fact]
    public void NuGetReferenceTypeTest()
    {
        IReference reference = new NuGetReference("ReferenceResolver", "1.0.0");
        Assert.Equal(ReferenceType.NuGetPackage, reference.ReferenceType);
        Assert.Equal($"nuget: {reference.Reference}", reference.ReferenceWithSchema);
    }

    [Fact]
    public void FileReferenceEqualsTest()
    {
        var reference1 = new FileReference("ReferenceResolver.dll");
        var reference2 = new FileReference("ReferenceResolver.dll");
        Assert.Equal(reference1, reference2);
    }

    [Fact]
    public void NuGetReferenceEqualsTest()
    {
        var reference1 = new NuGetReference("ReferenceResolver", "1.0.0");
        var reference2 = new NuGetReference("ReferenceResolver", "1.0.0");
        Assert.Equal(reference1, reference2);
    }
    
    [Fact]
    public async Task NuGetReferenceResolveTest()
    {
        var resolver = new NuGetReferenceResolver(new NuGetHelper(NullLoggerFactory.Instance));
        var result = await resolver.Resolve("WeihanLi.Npoi, 2.4.2", "net6.0")
            .ContinueWith(r => r.Result.ToArray());
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.False(string.IsNullOrEmpty(result[0]));
    }
}
