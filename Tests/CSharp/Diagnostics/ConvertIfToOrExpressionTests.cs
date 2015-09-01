using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConvertIfToOrExpressionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestVariableDeclarationCase()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		$if$ (o < 10)
			b = true;
	}
}", @"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10 || o < 10;
	}
}");
        }

        [Test]
        public void TestVariableDeclarationCaseWithComment()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		// Some comment
		bool b = o > 10;
		$if$ (o < 10)
			b = true;
	}
}", @"class Foo
{
	int Bar(int o)
	{
		// Some comment
		bool b = o > 10 || o < 10;
	}
}");
        }

        [Test]
        public void TestVariableDeclarationCaseBlock()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		$if$ (o < 10)
		{
			b = true;
		}
	}
}", @"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10 || o < 10;
	}
}");
        }

        [Test]
        public void TestCommonCase()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		$if$ (o < 10)
			b = true;
	}
}", @"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		b |= o < 10;
	}
}");
        }

        [Test]
        public void TestCommonCaseWithComment()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		// Some comment
		$if$ (o < 10)
			b = true;
	}
}", @"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		// Some comment
		b |= o < 10;
	}
}");
        }

        [Test]
        public void TestCommonCaseBlock()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		$if$ (o < 10)
		{
			b = true;
		}
	}
}", @"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		b |= o < 10;
	}
}");
        }

        [Test]
        public void TestCommonCaseWithElse()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
		Console.WriteLine ();
		if (o < 10)
		{
			b = true;
		}
		else
		{
			return 21;
		}
	}
}");
        }

        [Test]
        public void TestCommonCaseWithMemberAssignment()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		SomeType st = new SomeType();
		st.IsEnabled = o > 10;
		Console.WriteLine ();
		$if$ (o < 10)
			st.IsEnabled = true;
	}
}", @"class Foo
{
	int Bar(int o)
	{
		SomeType st = new SomeType();
		st.IsEnabled = o > 10;
		Console.WriteLine ();
		st.IsEnabled |= o < 10;
	}
}");
        }

        [Test]
        public void TestConversionBug()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	public override void VisitComposedType (ComposedType composedType)
	{
		$if$ (composedType.PointerRank > 0)
			unsafeStateStack.Peek ().UseUnsafeConstructs = true;
	}
}", @"class Foo
{
	public override void VisitComposedType (ComposedType composedType)
	{
		unsafeStateStack.Peek ().UseUnsafeConstructs |= composedType.PointerRank > 0;
	}
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	int Bar(int o)
	{
		bool b = o > 10;
#pragma warning disable " + CSharpDiagnosticIDs.ConvertIfToOrExpressionAnalyzerID + @"
		if (o < 10)
			b = true;
	}
}");
        }

        [Test]
        public void TestNullCheckBug()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
	public bool Enabled { get; set; }

	int Bar(Foo fileChangeWatcher)
	{
		if (fileChangeWatcher != null)
			fileChangeWatcher.Enabled = true;
	}
}");
        }

        [Test]
        public void TestNullCheckBug2()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
    class FooChild
    {
        public bool Enabled { get; set; }
    }
    public FooChild Child { get; set; }
	

	int Bar(Foo fileChangeWatcher)
	{
		if (fileChangeWatcher.Child != null)
			fileChangeWatcher.Child.Enabled = true;
	}
}");
        }

    }
}

