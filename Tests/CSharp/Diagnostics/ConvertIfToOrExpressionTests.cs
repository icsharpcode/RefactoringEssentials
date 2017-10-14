using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertIfToOrExpressionTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestComplexAssignmentCase()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Foo
{
    int Bar(bool foo)
    {
        bool bar = false;
        if (foo)
        {
            bar ^= true;
        }
    }
}");
        }

        [Fact]
        public void TestComplexMemberAssignment()
        {
            Analyze<ConvertIfToOrExpressionAnalyzer>(@"class Item
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
        if (selectedIndex > -1) { itemList.Rows[selectedIndex].Selected = true; }
    }
}");
        }
    }
}

