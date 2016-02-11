using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantUnsafeContextTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnsafeClass()
        {
            Test<RedundantUnsafeContextAnalyzer>(@"
unsafe class Foo
{
	public static void Main (string[] args)
	{
		System.Console.WriteLine (""Hello World!"");
	}
}
", @"
class Foo
{
	public static void Main (string[] args)
	{
		System.Console.WriteLine (""Hello World!"");
	}
}
");
        }

        [Test]
        public void TestUnsafeStatement()
        {
            Test<RedundantUnsafeContextAnalyzer>(@"
class Foo
{
	public static void Main (string[] args)
	{
		unsafe {
			System.Console.WriteLine (""Hello World1!"");
			System.Console.WriteLine (""Hello World2!"");
		}
	}
}
", @"
class Foo
{
	public static void Main (string[] args)
	{
		System.Console.WriteLine (""Hello World1!"");
		System.Console.WriteLine (""Hello World2!"");
	}
}
");
        }

        [Test]
        public void TestNestedUnsafeStatement()
        {
            Test<RedundantUnsafeContextAnalyzer>(@"
unsafe class Program
{
	static void Main(string str)
	{
		unsafe {
			fixed (char* charPtr = &str) {
				*charPtr = 'A';
			}
		}
	}
}
", @"
unsafe class Program
{
	static void Main(string str)
	{
		fixed (char* charPtr = &str) {
			*charPtr = 'A';
		}
	}
}
");
        }

        [Test]
        public void TestValidFixedPointer()
        {
            Analyze<RedundantUnsafeContextAnalyzer>(@"
unsafe struct Foo {
	public fixed char fixedBuffer[128];
}
");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<RedundantUnsafeContextAnalyzer>(@"
unsafe class Foo
{
	public static void Main (string[] args)
	{
		// ReSharper disable once RedundantUnsafeContext
		System.Console.WriteLine (""Hello World!"");
	}
}
");
        }

        [Test]
        public void TestSizeOf()
        {
            Analyze<RedundantUnsafeContextAnalyzer>(@"
public static class TestClass
{
	struct TestStruct {
	}
	public static void Main(String[] args)
	{
		unsafe {
			int a = sizeof(TestStruct);
		}
	}
}");
        }

        [Test]
        public void TestFixed()
        {
            Analyze<RedundantUnsafeContextAnalyzer>(@"
class Foo
{
	unsafe struct TestStruct
	{
		public fixed byte TestVar[32];
	}
}
");
        }
    }
}