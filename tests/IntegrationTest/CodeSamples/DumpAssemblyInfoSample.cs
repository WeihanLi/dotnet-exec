// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using System.Reflection;
using WeihanLi.Extensions.Dump;

System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Dump();
typeof(HostBuilder).Assembly.Location.Dump();
Assembly.GetExecutingAssembly()
    .GetReferencedAssemblies()
    .Select(ass=> Assembly.Load(ass))
    .Select(x=>x.Location)
    .Dump();
