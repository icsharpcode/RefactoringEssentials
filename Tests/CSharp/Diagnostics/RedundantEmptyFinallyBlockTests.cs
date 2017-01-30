using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantEmptyFinallyBlockTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestRedundantTry()
        {
            Analyze<RedundantEmptyFinallyBlockAnalyzer>(@"
using System;
class Test
{
    static void Main (string[] args)
    {
        try
        {
            Console.WriteLine(""1"");
            Console.WriteLine(""2"");
        } $finally$
        {
        }
    }
}
", @"
using System;
class Test
{
    static void Main (string[] args)
    {
        Console.WriteLine(""1"");
        Console.WriteLine(""2"");
    }
}
");
        }

        [Fact]
        public void TestSimpleCase()
        {
            Analyze<RedundantEmptyFinallyBlockAnalyzer>(@"
using System;
class Test
{
    static void Main (string[] args)
    {
        try
        {
            Console.WriteLine(""1"");
            Console.WriteLine(""2"");
        }
        catch (Exception)
        {
        } $finally$
        {
        }
    }
}
", @"
using System;
class Test
{
    static void Main (string[] args)
    {
        try
        {
            Console.WriteLine(""1"");
            Console.WriteLine(""2"");
        }
        catch (Exception)
        {
        }
    }
}
");
        }

        [Fact]
        public void TestInvalid()
        {
            Analyze<RedundantEmptyFinallyBlockAnalyzer>(@"
using System;
class Test
{
    static void Main(string[] args)
    {
        try {
            Console.WriteLine(""1"");
        } finally {
            Console.WriteLine(""2"");
        }
    }
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantEmptyFinallyBlockAnalyzer>(@"
using System;
class Test
{
    static void Main(string[] args)
    {
        try {
            Console.WriteLine(""1"");
            Console.WriteLine(""2"");
        }
#pragma warning disable " + CSharpDiagnosticIDs.RedundantEmptyFinallyBlockAnalyzerID + @"
         finally {
        }
    }
}
");
        }
    }
}

