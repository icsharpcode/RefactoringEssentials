using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class CS0162UnreachableCodeDetectedTests : CSharpCodeFixTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact(Skip="Not supported")]
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

        [Fact(Skip="Not supported")]
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

        [Fact]
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


        [Fact]
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

        [Fact]
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

		[Fact(Skip = "Not supported")]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact(Skip="Got broken due ast new line nodes")]
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

        [Fact(Skip="Broken.")]
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

        [Fact(Skip="Broken.")]
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
