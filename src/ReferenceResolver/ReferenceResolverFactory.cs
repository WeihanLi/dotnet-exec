// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging.Abstractions;

namespace ReferenceResolver;

public interface IReferenceResolverFactory
{
    IReferenceResolver GetResolver(ReferenceType referenceType);
}

public sealed class ReferenceResolverFactory
{
    public IReferenceResolver GetResolver(ReferenceType referenceType)
    {
        return referenceType switch
        {
            ReferenceType.LocalFile => new FileReferenceResolver(),
            ReferenceType.LocalFolder => new FolderReferenceResolver(),
            ReferenceType.NuGetPackage => new NuGetReferenceResolver(NullLoggerFactory.Instance),
            ReferenceType.FrameworkReference => new FrameworkReferenceResolver(),
            _ => throw new ArgumentOutOfRangeException(nameof(referenceType))
        };
    }
}
