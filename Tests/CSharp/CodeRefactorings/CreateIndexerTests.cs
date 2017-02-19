using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class CreateIndexerTests : CSharpCodeRefactoringTestBase
    {
        public void TestCreateIndexer(string input, string output)
        {
            string result = RunContextAction(new CreateIndexerAction(), HomogenizeEol(input));
            output = HomogenizeEol(output);
            bool passed = result == output;
            if (!passed)
            {
                Console.WriteLine("-----------Expected:");
                Console.WriteLine(output);
                Console.WriteLine("-----------Got:");
                Console.WriteLine(result);
            }
            Assert.Equal(output, result);
        }


        [Fact(Skip="Needs insertion cursor mode.")]
        public void TestIndexer()
        {
            TestCreateIndexer(
@"
class TestClass
{
	void TestMethod ()
	{
		$this[0] = 2;
	}
}
", @"
class TestClass
{
	int this [int i] {
		get {
			throw new System.NotImplementedException ();
		}
		set {
			throw new System.NotImplementedException ();
		}
	}
	void TestMethod ()
	{
		this[0] = 2;
	}
}
");
        }
        [Fact(Skip="Needs insertion cursor mode.")]
        public void TestInterfaceIndexer()
        {
            TestCreateIndexer(
@"
interface FooBar
{
}

class TestClass
{
	void TestMethod ()
	{
		FooBar fb;
		$fb[0] = 2;
	}
}
", @"
interface FooBar
{
	int this [int i] {
		get;
		set;
	}
}

class TestClass
{
	void TestMethod ()
	{
		FooBar fb;
		fb[0] = 2;
	}
}
");
        }

        [Fact(Skip="Needs insertion cursor mode.")]
        public void TestExternIndexer()
        {
            TestCreateIndexer(
@"
class FooBar
{
}

class TestClass
{
	void TestMethod ()
	{
		FooBar fb;
		$fb[0] = 2;
	}
}
", @"
class FooBar
{
	public int this [int i] {
		get {
			throw new System.NotImplementedException ();
		}
		set {
			throw new System.NotImplementedException ();
		}
	}
}

class TestClass
{
	void TestMethod ()
	{
		FooBar fb;
		fb[0] = 2;
	}
}
");
        }

        [Fact(Skip="Needs insertion cursor mode.")]
        public void TestindexerInFrameworkClass()
        {
            TestWrongContext<CreateIndexerAction>(
@"class TestClass
{
	void TestMethod ()
	{
		$new System.Buffer ()[0] = 2;
	}
}
");
        }

        [Fact(Skip="Needs insertion cursor mode.")]
        public void TestEnumCase()
        {
            TestWrongContext<CreateIndexerAction>(@"
enum AEnum { A }
class Foo
{
	public void Test ()
	{
		AEnum e;
		$e[0] = 2;
	}
}
");
        }
    }
}

