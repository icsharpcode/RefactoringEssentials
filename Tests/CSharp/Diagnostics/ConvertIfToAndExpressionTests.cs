using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConvertIfToAndExpressionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestVariableDeclarationCase()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        $if$ (o < 10)
            b = false;
    }
}", @"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10 && o >= 10;
    }
}");
        }

        [Test]
        public void TestVariableDeclarationCaseAndComment()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        // Some comment
        bool b = o > 10;
        $if$ (o < 10)
            b = false;
    }
}", @"class Foo
{
    int Bar(int o)
    {
        // Some comment
        bool b = o > 10 && o >= 10;
    }
}");
        }

        [Test]
        public void TestVariableDeclarationCaseWithBlock()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        $if$ (o < 10)
        {
            b = false;
        }
    }
}", @"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10 && o >= 10;
    }
}");
        }

        [Test]
        public void TestComplexVariableDeclarationCase()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10 || o < 10;
        $if$ (o < 10)
            b = false;
    }
}", @"class Foo
{
    int Bar(int o)
    {
        bool b = (o > 10 || o < 10) && o >= 10;
    }
}");
        }

        [Test]
        public void TestConversionBug()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    public override void VisitComposedType (ComposedType composedType)
    {
        $if$ (composedType.PointerRank > 0)
            unsafeStateStack.Peek ().UseUnsafeConstructs = false;
    }
}", @"class Foo
{
    public override void VisitComposedType (ComposedType composedType)
    {
        unsafeStateStack.Peek ().UseUnsafeConstructs &= composedType.PointerRank <= 0;
    }
}");
        }

        [Test]
        public void TestCommonCase()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        $if$ (o < 10)
            b = false;
    }
}", @"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        b &= o >= 10;
    }
}");
        }

        [Test]
        public void TestCommonCaseWithComment()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        // Some comment
        $if$ (o < 10)
            b = false;
    }
}", @"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        // Some comment
        b &= o >= 10;
    }
}");
        }

        [Test]
        public void TestCommonCaseWithBlock()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        $if$ (o < 10)
        {
            b = false;
        }
    }
}", @"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        b &= o >= 10;
    }
}");
        }

        [Test]
        public void TestCommonCaseWithElse()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(int o)
    {
        bool b = o > 10;
        Console.WriteLine ();
        if (o < 10)
        {
            b = false;
        }
        else
        {
            return 42;
        }
    }
}");
        }

        [Test]
        public void TestNullCheckBug()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
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
        public void TestComplexAssignmentCase()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Foo
{
    int Bar(bool foo)
    {
        bool bar = true;
        if (foo)
        {
            bar ^= false;
        }
    }
}");
        }

        [Test]
        public void TestComplexMemberAssignment()
        {
            Analyze<ConvertIfToAndExpressionAnalyzer>(@"class Item
{
    public bool Selected;
}

class ItemList
{
    public List<Item> Rows = new List<Item>();
}

class Foo
{
    int selectedIndex;

    int Bar(ItemList itemList)
    {
        if (selectedIndex > -1) { itemList.Rows[selectedIndex].Selected = false; }
    }
}");
        }
    }
}

