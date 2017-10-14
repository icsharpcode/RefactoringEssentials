using RefactoringEssentials.CSharp.CodeFixes;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeFixes
{
    public class InvalidConversionTests : CSharpCodeFixTestBase
    {
        [Fact]
        public void TestConversion()
        {
            var input = @"
class TestClass
{
enum Enum{ };
    void TestMethod (Enum i)
    {
        int x;
        x = i;
    }
}";
            var output = @"
class TestClass
{
enum Enum{ };
    void TestMethod (Enum i)
    {
        int x;
        x = (int)i;
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestConversionInInitializer()
        {
            var input = @"
class TestClass
{
enum Enum{ };
    void TestMethod (Enum i)
    {
        int x = i;
    }
}";
            var output = @"
class TestClass
{
enum Enum{ };
    void TestMethod (Enum i)
    {
        int x = (int)i;
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestConversionDoubleFloat()
        {
            var input = @"
class Foo
{
    void Bar () {
        double c = 3.5;
        float fc;
        fc = c;
    }
}";
            var output = @"
class Foo
{
    void Bar () {
        double c = 3.5;
        float fc;
        fc = (float)c;
    }
}";

            Test<InvalidConversionCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestConversionEnumToInt()
        {
            var input = @"
class Foo
{
    enum Enum { Zero }
    void Bar () {
        var e = Enum.Zero;
        int val;
        val = e;
    }
}";
            var output = @"
class Foo
{
    enum Enum { Zero }
    void Bar () {
        var e = Enum.Zero;
        int val;
        val = (int)e;
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }


        [Fact]
        public void AssignCustomClassToString()
        {
            Test<InvalidConversionCodeFixProvider>(@"
class TestClass
{
    void TestMethod ()
    {
        string x = this;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        var x = this;
    }
}");
        }

        [Fact]
        public void TestReturnInMethod()
        {
            var input = @"
class TestClass
{
    enum Enum{ };
    int TestMethod (Enum i)
    {
        return i;
    }
}";
            var output = @"
class TestClass
{
    enum Enum{ };
    int TestMethod (Enum i)
    {
        return (int)i;
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestReturnInMethodChangeReturnType()
        {
            var input = @"
class TestClass
{
    int TestMethod ()
    {
        return ""foo"";
    }
}";
            var output = @"
class TestClass
{
    string TestMethod ()
    {
        return ""foo"";
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }


        [Fact]
        public void TestReturnInAnonymousMethod()
        {
            var input = @"using System;

class TestClass
{
    enum Enum{ };
    void TestMethod (Enum i)
    {
        Func<int> foo = delegate {
            return i;
        };
    }
}";
            var output = @"using System;

class TestClass
{
    enum Enum{ };
    void TestMethod (Enum i)
    {
        Func<int> foo = delegate {
            return (int)i;
        };
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }


        [Fact]
        public void TestReturnInProperty()
        {
            var input = @"
class TestClass
{
    enum Enum{ A };
    int Test {
        get {
            return Enum.A;
        }
    }
}";
            var output = @"
class TestClass
{
    enum Enum{ A };
    int Test {
        get {
            return (int)Enum.A;
        }
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestCall()
        {
            var input = @"
class TestClass
{
    enum Enum{ };
    void Foo(string s, int i) {}
    void TestMethod (Enum i)
    {
        Foo (""Bar"", i);
    }
}";
            var output = @"
class TestClass
{
    enum Enum{ };
    void Foo(string s, int i) {}
    void TestMethod (Enum i)
    {
        Foo (""Bar"", (int)i);
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }



        [Fact]
        public void TestArrayInitializer()
        {
            var input = @"
class TestClass
{
    enum Enum{ A }
    public static void Main (string[] args)
    {
        System.Console.WriteLine (new int[] { Enum.A });
    }
}";
            var output = @"
class TestClass
{
    enum Enum{ A }
    public static void Main (string[] args)
    {
        System.Console.WriteLine (new int[] { (int)Enum.A });
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }



        [Fact]
        public void TestBinaryOperator()
        {
            var input = @"
class TestClass
{
enum Enum{ };
    void TestMethod (ulong i)
    {
        int x;
        x = i + i;
    }
}";
            var output = @"
class TestClass
{
enum Enum{ };
    void TestMethod (ulong i)
    {
        int x;
        x = (int)(i + i);
    }
}";
            Test<InvalidConversionCodeFixProvider>(input, output);
        }

        [Fact]
        public void TestDeclarationFix()
        {
            Test<InvalidConversionCodeFixProvider>(@"
using System.Collections.Generic;
class TestClass
{
    string[] str = new List<string> ();
}", @"
using System.Collections.Generic;
class TestClass
{
    List<string> str = new List<string> ();
}");
        }

        [Fact]
        public void TestLocalDeclarationFix()
        {
            Test<InvalidConversionCodeFixProvider>(@"
using System.Collections.Generic;
class TestClass
{
    void Foo ()
    {
        string[] str = new List<string> ();
    }
}", @"
using System.Collections.Generic;
class TestClass
{
    void Foo ()
    {
        var str = new List<string> ();
    }
}");
        }

    }
}