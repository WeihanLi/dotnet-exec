// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Reflection;
using WeihanLi.Extensions.Dump;

System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Dump();
Assembly.GetExecutingAssembly()
    .GetReferencedAssemblies()
    .Select(ass=> Assembly.Load(ass))
    .Select(x=> x.Location)
    .Dump();
