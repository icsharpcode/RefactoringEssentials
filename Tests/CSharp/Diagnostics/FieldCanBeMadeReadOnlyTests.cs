using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    public class FieldCanBeMadeReadOnlyTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestInitializedFieldAssignedInAnotherClassPart()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"partial class Test
{
    object fooBar = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}
class Test
{
    public void SomeMethod()
    {
        fooBar = null;
    }
}
");
        }

        [Fact]
        public void TestInitializedStaticField()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    static object $fooBar$ = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}", @"class Test
{
    static readonly object fooBar = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}");
        }

        [Fact]
        public void TestInitializedStaticFieldUsedInInstanceMember()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    static object $fooBar$ = new object();
    public void TestMethod()
    {
        Console.WriteLine(fooBar);
    }
}", @"class Test
{
    static readonly object fooBar = new object();
    public void TestMethod()
    {
        Console.WriteLine(fooBar);
    }
}");
        }

        [Fact]
        public void TestStaticFieldAssignedInStaticConstructor()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    static object $fooBar$;
    static Test()
    {
        fooBar = new object();
    }
}", @"class Test
{
    static readonly object fooBar;
    static Test()
    {
        fooBar = new object();
    }
}");
        }

        [Fact]
        public void TestStaticFieldAlsoAssignedInInstanceConstructor()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    static object fooBar;
    static Test()
    {
        fooBar = new object();
    }
    public Test()
    {
        fooBar = new object();
    }
}");
        }

        [Fact]
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


        [Fact]
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

        [Fact]
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


        [Fact]
        public void TestUninitializedValueTypeField()
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

        [Fact]
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


        [Fact]
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


        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestUnassignedField()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    object fooBar;
}");
        }

        [Fact]
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

        /// <summary>
        /// Bug 19832 - Source Analysis should probably not suggest making a serialized field readonly
        /// </summary>
        [Fact]
        public void TestBug19832()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"
using System;
class Test
{
    [Serializable]
    object fooBar = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}
");
        }

        [Fact(Skip="broken")]
        public void TestComments()
        {
            Analyze<FieldCanBeMadeReadOnlyAnalyzer>(@"class Test
{
    /// <summary>test</summary> 
    object $fooBar$ = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}", @"class Test
{
    /// <summary>test</summary>
    readonly object fooBar = new object();
    public static void Main(string[] args)
    {
        Console.WriteLine(fooBar);
    }
}");
        }

    }
}

