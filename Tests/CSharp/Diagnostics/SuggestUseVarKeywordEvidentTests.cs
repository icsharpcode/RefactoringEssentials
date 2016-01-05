using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class SuggestUseVarKeywordEvidentTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInspectorCase1()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
		$Foo$ foo = (Foo)o;
	}
}", @"class Foo
{
	void Bar (object o)
	{
		var foo = (Foo)o;
	}
}");
        }

        [Test]
        public void TestV2()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
		$Foo$ foo = (Foo)o;
	}
}", @"class Foo
{
	void Bar (object o)
	{
		var foo = (Foo)o;
	}
}");
        }

        [Test]
        public void When_Creating_An_Object()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
		$Foo$ foo = new Foo();
	}
}", @"class Foo
{
	void Bar (object o)
	{
		var foo = new Foo();
	}
}");
        }

        [Test]
        public void WithDynamic()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
		dynamic foo = new Foo();
	}
}");
        }

        [Test]
        public void When_Explicitely_Initializing_An_Array()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
	    $int[]$ foo = new int[] { 1, 2, 3 };
	}
}", @"class Foo
{
	void Bar (object o)
	{
	    var foo = new int[] { 1, 2, 3 };
	}
}");

        }

        [Test]
        public void When_Implicitely_Initializing_An_Array()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
	    int[] foo = new[] { 1, 2, 3 };
	}
}");
        }

        [Test]
        public void When_Retrieving_Object_By_Property()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"
   
public class SomeClass
{
     public SomeClass MyProperty { get; set; }
 }

public class Foo
{
    public void SomeMethod(object o)
    {
        $SomeClass$ someObject = (SomeClass)o;
        SomeClass retrievedObject = someObject.MyProperty;
    }
}
", @"
   
public class SomeClass
{
     public SomeClass MyProperty { get; set; }
 }

public class Foo
{
    public void SomeMethod(object o)
    {
        var someObject = (SomeClass)o;
        SomeClass retrievedObject = someObject.MyProperty;
    }
}
");
        }
        [Test]
        public void When_Casting_Objects()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"
public class MyClass
{
}

public class Foo
{
    public void SomeMethod(object o)
    {
        $MyClass$ someObject = (MyClass)o;
        if(someObject is MyClass)
            $MyClass$ castedObject = o as MyClass;
    }
}
", @"
public class MyClass
{
}

public class Foo
{
    public void SomeMethod(object o)
    {
        var someObject = (MyClass)o;
        if(someObject is MyClass)
            var castedObject = o as MyClass;
    }
}
");
        }

        [Test]
        public void TestNoInitializer()
        {
            Analyze<SuggestUseVarKeywordEvidentAnalyzer>(@"class Foo
{
	void Bar (object o)
	{
		Foo foo;
	}
}");
        }
    }
}