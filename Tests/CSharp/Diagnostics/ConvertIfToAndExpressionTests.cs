using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertIfToAndExpressionTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

