// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace CSharp12Sample
{
    public static class InterceptorSample
    {
        public static void MainTest()
        {
            var a = new A();
            a.TestMethod();
        }
    }
    
    public class A
    {
        public void TestMethod()
        {
            Console.WriteLine("A.TestMethod");
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
#pragma warning disable CS9113 // Parameter is unread.
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
#pragma warning restore CS9113 // Parameter is unread.
    {
    }
}

namespace CSharp12Sample.Generated
{
    public static class Extensions
    {
        [System.Runtime.CompilerServices.InterceptsLocation(
            @"C:\projects\sources\dotnet-exec\artifacts\bin\IntegrationTest\debug\CodeSamples\InterceptorSample.cs", 
            line: 11, character: 15)
        ]
        public static void TestMethodInterceptor(this A a)
        {
            Console.WriteLine($"Intercepted: {nameof(TestMethodInterceptor)}");
        }
    }
}
