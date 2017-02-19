using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantStringToCharArrayCallTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestSimpleForeachCase()
        {
            Analyze<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (char c in str.$ToCharArray ()$) {
			Console.WriteLine (c);
		}
	}
}
", @"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (char c in str) {
			Console.WriteLine (c);
		}
	}
}
");
        }

        [Fact]
        public void TestForeachEachWichParametrizedCharToArray()
        {
            Analyze<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (char c in str.ToCharArray (1, 5)) {
			Console.WriteLine (c);
		}
	}
}
");
        }

        [Fact]
        public void TestVarForeachCase()
        {
            Analyze<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (var c in str.$ToCharArray ()$) {
			Console.WriteLine (c);
		}
	}
}
", @"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (var c in str) {
			Console.WriteLine (c);
		}
	}
}
");
        }

        [Fact]
        public void TestIndexerCase()
        {
            Analyze<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		Console.WriteLine (str.$ToCharArray ()$[5]);
	}
}
", @"
using System;
class FooBar
{
	public void Test (string str)
	{
		Console.WriteLine (str[5]);
	}
}
");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		// ReSharper disable once RedundantStringToCharArrayCall
+#pragma warning disable " + CSharpDiagnosticIDs.RedundantStringToCharArrayCallAnalyzerID + @"
		foreach (char c in str.ToCharArray ()) {
			Console.WriteLine (c);
		}
	}
}
");
        }


    }
}

