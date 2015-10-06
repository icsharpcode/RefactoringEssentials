using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantStringToCharArrayCallTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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


        [Test]
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

