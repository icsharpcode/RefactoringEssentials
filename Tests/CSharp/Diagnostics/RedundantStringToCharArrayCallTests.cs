using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantStringToCharArrayCallTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleForeachCase()
        {
            Test<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (char c in str.ToCharArray ()) {
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
        public void TestVarForeachCase()
        {
            Test<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		foreach (var c in str.ToCharArray ()) {
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
            Test<RedundantStringToCharArrayCallAnalyzer>(@"
using System;
class FooBar
{
	public void Test (string str)
	{
		Console.WriteLine ((str.ToCharArray ())[5]);
	}
}
", @"
using System;
class FooBar
{
	public void Test (string str)
	{
		Console.WriteLine (str [5]);
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
		foreach (char c in str.ToCharArray ()) {
			Console.WriteLine (c);
		}
	}
}
");
        }


    }
}

