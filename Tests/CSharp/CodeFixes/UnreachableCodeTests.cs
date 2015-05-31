using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeFixes;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    [TestFixture]
    public class CS0162UnreachableCodeDetectedTests : CSharpCodeFixTestBase
    {
        [Test]
        public void TestReturn()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		return;
		int a = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		return;
	}
}");
        }

        [Test]
        public void TestBreak()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		while (true) {
			break;
			int a = 1;
		}
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		while (true) {
			break;
		}
	}
}");
        }

        [Ignore("Not supported")]
        [Test]
        public void TestRedundantGoto()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		goto Foo; Foo:
		int a = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		goto Foo; Foo:
	}
}");
        }

        [Ignore("Not supported")]
        [Test]
        public void TestGotoUnreachableBlock()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		int x = 1;
		goto Foo;
		{
			x = 2;
			Foo:
			x = 3;
		}
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int x = 1;
		goto Foo;
		{
			Foo:
			x = 3;
		}
	}
}");
        }

        [Test]
        public void TestContinue()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		while (true) {
			continue;
			break;
		}
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		while (true) {
			continue;
		}
	}
}");
        }


        [Test]
        public void TestFor()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		for (int i = 0; i < 10; i++) {
			break;
		}
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		for (int i = 0; i < 10; ) {
			break;
		}
	}
}");
        }

        [Test]
        public void TestConstantCondition()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		if (true) {
			return;
		}
		int a = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		if (true) {
			return;
		}
	}
}");
        }

        [Ignore("Not supported.")]
        [Test]
        public void TestConditionalExpression()
        {
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		int a = 1;
	}
}";
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		int a = true ? 1 : 0;
	}
}", output);
        }

        [Test]
        public void TestInsideLambda()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action action = () => {
			return;
			int a = 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Action action = () => {
			return;
		};
	}
}");
        }

        [Test]
        public void TestInsideAnonymousMethod()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		System.Action action = delegate () {
			return;
			int a = 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		System.Action action = delegate () {
			return;
		};
	}
}");
        }

        [Test]
        public void TestIgnoreLambdaBody()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		return;
		System.Action action = () => {
			return;
			int a = 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		return;
		System.Action action = () => {
			return;
		};
	}
}");
        }

        [Test]
        public void TestIgnoreAnonymousMethodBody()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		return;
		System.Action action = delegate() {
			return;
			int a = 1;
		};
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		return;
		System.Action action = delegate() {
			return;
		};
	}
}");
        }

        [Test]
        public void TestGroupMultipleStatements()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		return;
		int a = 1;
		a++;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		return;
		a++;
	}
}");
        }

        [Test]
        public void TestRemoveCode()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		return;
		int a = 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		return;
	}
}", 0);
        }

        [Ignore("Got broken due ast new line nodes")]
        [Test]
        public void TestCommentCode()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		return;
		int a = 1;
		a++;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		return;
/*
		int a = 1;
		a++;
*/
	}
}";
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(input, output, 1);
        }

        [Ignore("Broken.")]
        [Test]
        public void TestIfTrueBranch()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		if (true) {
			System.Console.WriteLine (1);
		} else {
			System.Console.WriteLine (2);
		}
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		if (true) {
			System.Console.WriteLine (1);
		}
	}
}");
        }

        [Ignore("Broken.")]
        [Test]
        public void TestIfFalseBranch()
        {
            Test<CS0162UnreachableCodeDetectedCodeFixProvider>(@"
class TestClass
{
	void TestMethod ()
	{
		if (false) {
			System.Console.WriteLine (1);
		} else {
			System.Console.WriteLine (2);
		}
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		{
			System.Console.WriteLine (2);
		}
	}
}");
        }

    }
}
