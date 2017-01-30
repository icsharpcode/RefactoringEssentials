using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertToConstantTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBasicCase()
        {
            Test<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		Console.WriteLine (fooBar);
	}
}", @"class Test
{
	public static void Main (string[] args)
	{
		const int fooBar = 12;
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestWrongLocalType()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		object fooBar = 12;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingVariableCase1()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		Console.WriteLine (fooBar++);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingVariableCase2()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		Something (out fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingVariableCase3()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		if (args.Length == 10)
			fooBar = 20;
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingVariableCase4()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		switch (args[0]) {
			case ""Hello"":
				fooBar = 20;
				break;
		}
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingVariableCase5()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		fooBar += 1;
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingVariableCase6()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		int fooBar = 12;
		TestMe (ref fooBar);
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		// ReSharper disable once ConvertToConstant.Local
		int fooBar = 12;
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestField()
        {
            Test<ConvertToConstantAnalyzer>(@"class Test
{
	int fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}", @"class Test
{
	const int fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestReadonlyField()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	readonly int fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestStaticField()
        {
            Test<ConvertToConstantAnalyzer>(@"class Test
{
	static int fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}", @"class Test
{
	const int fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingField()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public Test ()
	{
		fooBar = 2323;
	}
	int fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestWrongFieldType()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	object fooBar = 12;
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingFieldCase2()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	int fooBar = 12;
	public Test ()
	{
		this.fooBar = 12;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestChangingFieldCase3()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	int fooBar = 12;
	public Test ()
	{
		++this.fooBar;
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestShadowedField()
        {
            Test<ConvertToConstantAnalyzer>(@"class Test
{
	int fooBar = 12;

	public Test (int fooBar)
	{
		fooBar = 2323;
	}

	public void Bar ()
	{
		Console.WriteLine (fooBar);
	}
}", @"class Test
{
	const int fooBar = 12;

	public Test (int fooBar)
	{
		fooBar = 2323;
	}

	public void Bar ()
	{
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestShadowedFieldCase2()
        {
            Test<ConvertToConstantAnalyzer>(@"class Test
{
	int fooBar = 12;

	public Test ()
	{
		int fooBar;
		fooBar = 2323;
	}

	public void Bar ()
	{
		Console.WriteLine (fooBar);
	}
}", @"class Test
{
	const int fooBar = 12;

	public Test ()
	{
		int fooBar;
		fooBar = 2323;
	}

	public void Bar ()
	{
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNeverSuggestForControlVariable()
        {
            Analyze<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		for (int i = 0; i < 10;) {
			Console.WriteLine(i);
		}
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVarCase()
        {
            Test<ConvertToConstantAnalyzer>(@"class Test
{
	public static void Main (string[] args)
	{
		var fooBar = 12;
		Console.WriteLine (fooBar);
	}
}", @"class Test
{
	public static void Main (string[] args)
	{
		const int fooBar = 12;
		Console.WriteLine (fooBar);
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestArbitraryStructCase()
        {
            Analyze<ConvertToConstantAnalyzer>(@"
struct Bar {
	public int A;
}

class Test
{
	public static void Main (string[] args)
	{
		var fooBar = default(Bar);
		Console.WriteLine (fooBar);
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestArbitraryStructCase2()
        {
            Analyze<ConvertToConstantAnalyzer>(@"
struct Bar {
	public int A;
}

class Test
{
	const Bar foo = new Bar();
	public static void Main (string[] args)
	{
		var fooBar = foo;
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestComplexCase()
        {
            Test<ConvertToConstantAnalyzer>(@"
class Test
{
	public static void Main (string[] args)
	{
		var pi2 = System.Math.PI * 2;
		Console.WriteLine (pi2);
	}
}", @"
class Test
{
	public static void Main (string[] args)
	{
		const double pi2 = System.Math.PI * 2;
		Console.WriteLine (pi2);
	}
}");
        }

    }
}

