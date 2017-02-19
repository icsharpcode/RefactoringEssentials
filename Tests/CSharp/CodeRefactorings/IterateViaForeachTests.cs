using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class IterateViaForeachTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void HandlesNonGenericCase()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections;
class TestClass
{
    public void F()
    {
        var $list = new ArrayList();
    }
}", @"
using System.Collections;
class TestClass
{
    public void F()
    {
        var list = new ArrayList();
        foreach (var item in list)
        {
        }
    }
}");
        }

        [Fact]
        public void HandlesExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        GetInts$();
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        foreach (var item in GetInts())
        {
        }
    }
}");
        }

        [Fact]
        public void HandlesAssignmentExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        IEnumerable<int> ints;
        $ints = GetInts();
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        IEnumerable<int> ints;
        ints = GetInts();
        foreach (var item in ints)
        {
        }
    }
}");
        }

        [Fact]
        public void HandlesStringPropertyAssignmentExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class Label
{
    public string Text { get; set; }
}

class TestClass
{
    public void F()
    {
        Label label = new Label();
        label.$Text = ""Some text"";
    }
}", @"
using System.Collections.Generic;
class Label
{
    public string Text { get; set; }
}

class TestClass
{
    public void F()
    {
        Label label = new Label();
        label.Text = ""Some text"";
        foreach (var item in label.Text)
        {
        }
    }
}");
        }

        [Fact]
        public void HandlesAsExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    public void F()
    {
        object s = """";
        s as IEnumerable$<char>;
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    public void F()
    {
        object s = """";
        foreach (var item in s as IEnumerable<char>)
        {
        }
    }
}", 0, false);
        }

        [Fact]
        public void NonKnownTypeNamingTest()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    public void F()
    {
        var $items = new List<TestClass> ();
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    public void F()
    {
        var items = new List<TestClass> ();
        foreach (var item in items)
        {
        }
    }
}");
        }

        [Fact]
        public void HandlesAsExpression()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    public void F()
    {
        object s = """";
        s as IEnumerable$<char>
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    public void F()
    {
        object s = """";
        foreach (var item in s as IEnumerable<char>)
        {
        }
    }
}", 0, true);
        }

        [Fact]
        public void HandlesLinqExpressionAssignment()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        var $filteredInts = from item in GetInts ()
                            where item > 0
                            select item;
    }
}", @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        var filteredInts = from item in GetInts ()
                            where item > 0
                            select item;
        foreach (var item in filteredInts)
        {
        }
    }
}");
        }

        [Fact]
        public void IgnoresExpressionsInForeachStatement()
        {
            TestWrongContext<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    public IEnumerable<int> GetInts()
    {
        return new int[] { };
    }

    public void F()
    {
        foreach (var i in $GetInts ()) {
        }
    }
}");
        }

        [Fact]
        public void IgnoresInitializersInForStatement()
        {
            TestWrongContext<IterateViaForeachAction>(@"
class TestClass
{
    public void F()
    {
        for (int[] i = new $int[] {} ;;) {
        }
    }
}");
        }

        [Fact]
        public void AddsForToBodyOfUsingStatement()
        {
            Test<IterateViaForeachAction>(@"
class TestClass
{
    public void F()
    {
        using (int[] $i = new int[] {})
        {
            Console.WriteLine(42);
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        using (int[] i = new int[] {})
        {
            foreach (var item in i)
            {
            }

            Console.WriteLine(42);
        }
    }
}");
        }

        [Fact]
        public void AddsBlockStatementToUsingStatement()
        {
            Test<IterateViaForeachAction>(@"
class TestClass
{
    public void F()
    {
        using (int[] $i = new int[] {});
    }
}", @"
class TestClass
{
    public void F()
    {
        using (int[] i = new int[] {})
        {
            foreach (var item in i)
            {
            }
        }
    }
}");
        }

        [Fact]
        public void ConvertsSingleStatementInUsingToBlock()
        {
            Test<IterateViaForeachAction>(@"
class TestClass
{
    public void F()
    {
        using (int[] $i = new int[] {})
            Console.WriteLine(42);
    }
}", @"
class TestClass
{
    public void F()
    {
        using (int[] i = new int[] {})
        {
            foreach (var item in i)
            {
            }

            Console.WriteLine(42);
        }
    }
}");
        }

        [Fact]
        public void IgnoresFieldDeclarations()
        {
            TestWrongContext<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    List<int> list$ = new List<int>();
}");
        }

        [Fact]
        public void HandlesLocalDeclarationWithObjectCreation()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
    void Method()
    {
        List<int> list = $new List<int>();
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    void Method()
    {
        List<int> list = new List<int>();
        foreach (var item in list)
        {
        }
    }
}");
        }
    }
}

