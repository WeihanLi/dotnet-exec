// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using WeihanLi.Extensions.Dump;

System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Dump();
Assembly.GetExecutingAssembly()
    .GetReferencedAssemblies()
    .Select(ass=> Assembly.Load(ass))
    .Select(x=> x.Location)
    .Dump();
