using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class FieldCanBeMadeReadOnlyTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInitializedField()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    object $fooBar$ = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}", @"class Test
{
    readonly object fooBar = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}");
        }

        [Test]
        public void TestFieldAssignedInConstructor()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    object $fooBar$;
    public Test()
    {
        fooBar = new object();
    }
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}", @"class Test
{
    readonly object fooBar;
    public Test()
    {
        fooBar = new object();
    }
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
#pragma warning disable " + CSharpDiagnosticIDs.FieldCanBeMadeReadOnlyAnalyzerID + @"
    object fooBar = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}");
        }


        [Test]
        public void TestFactoryMethod()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    object fooBar;
    
    public static Test Create()
    {
        var result = new Test();
        result.fooBar = new object();
        return result;
    }
}");
        }

        [Test]
        public void TestFactoryMethodCase2()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    object fooBar;
    
    public static Test Create()
    {
        var result = new Test {fooBar = new object() };
        return result;
    }
}");
        }


        [Test]
        public void TestUninitalizedValueTypeField()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    int $fooBar$;
    public Test()
    {
        fooBar = 5;
    }
}", @"class Test
{
    readonly int fooBar;
    public Test()
    {
        fooBar = 5;
    }
}");
        }

        [Test]
        public void TestInitalizedValueTypeField()
        {
            // Is handled by the 'to const' issue.
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    int fooBar = 12;
    public void FooBar()
    {
        System.Console.WriteLine(fooBar);
    }
}");
        }


        [Test]
        public void TestSpecializedFieldBug()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"
using System;
class Test<T> where T : IDisposable
{
    object fooBar = new object();
    public void Foo()
    {
        fooBar = null;
    }
}");
        }


        [Test]
        public void TestFieldAssignedInConstructorLambda()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"
using System;

class Test
{
    object fooBar;
    public Action<object> act;
    public Test()
    {
        act = o => { fooBar = o; };
    }
}");
        }

        [Test]
        public void MutableStruct()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    MutableStruct m;
    public static void Main(string[] args)
    {
        m.Increment();
    }
}
struct MutableStruct {
    int val;
    public void Increment() {
        val++;
    }
}
");
        }

        [Test]
        public void TestUnassignedField()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    object fooBar;
}");
        }

        [Test]
        public void TestMultiple()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    int $fooBar$, test = 5;
    public Test()
    {
        fooBar = 5;
    }
}", @"class Test
{
    int test = 5;
    readonly int fooBar;
    public Test()
    {
        fooBar = 5;
    }
}");
        }


        [Test]
        public void TestIssue37()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class SomeClass
{
    static int StaticField;
    public SomeClass()
    {
        StaticField = 1;
    }
}");
        }
    }
}

