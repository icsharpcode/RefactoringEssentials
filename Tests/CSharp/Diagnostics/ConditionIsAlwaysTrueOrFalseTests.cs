using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConditionIsAlwaysTrueOrFalseTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestComparsionWithNull()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		if ($i == null$) {
		}
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		if (false) {
		}
	}
}
");
        }


        [Fact]
        public void TestComparsionWithNullCase2()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
enum Bar { A, B }
class Test
{
	void Foo(Bar i)
	{
		if ($i != null$) {
		}
	}
}
", @"
enum Bar { A, B }
class Test
{
	void Foo(Bar i)
	{
		if (true) {
		}
	}
}
");
        }

        [Fact]
        public void TestComparisonArgumentWithNull()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
    void SomeMethod(bool condition)
    {
    }

    void Foo(int i)
    {
        SomeMethod($i == null$);
    }
}
", @"
class Test
{
    void SomeMethod(bool condition)
    {
    }

    void Foo(int i)
    {
        SomeMethod(false);
    }
}
");
        }

        [Fact]
        public void TestComparison()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		if ($1 > 2$) {
		}
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		if (false) {
		}
	}
}
");
        }

        [Fact]
        public void TestUnary()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		if ($!true$) {
		}
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		if (false) {
		}
	}
}
");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ConditionIsAlwaysTrueOrFalseAnalyzerID + @"
		if (i == null) {
		}
	}
}
");
        }


        [Fact]
        public void CompareWithNullable()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
class Bar
{
	public void Test(int? a)
	{
		if (a != null) {

		}
	}
}
");
        }

        [Fact]
        public void UserDefinedOperatorsNoReferences()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
struct Foo 
{
	public static bool operator ==(Foo value, Foo o)
	{
		return false;
	}

	public static bool operator !=(Foo value, Foo o)
	{
		return false;
	}
}

class Bar
{
	public void Test(Foo a)
	{
		if ($a != null$) {

		}
	}
}
");
        }

        [Fact]
        public void UserDefinedOperators()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
struct Foo 
{
	public static bool operator ==(Foo value, object o)
	{
		return false;
	}

	public static bool operator !=(Foo value, object o)
	{
		return false;
	}
}

class Bar
{
	public void Test(Foo a)
	{
		if (a != null) {

		}
	}
}
");
        }


        /// <summary>
        /// Bug 15099 - Wrong always true context
        /// </summary>
        [Fact]
        public void TestBug15099()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
struct Foo 
{
    string name;

    public Foo (string name)
    {
        this.name = name;
    }

    public static bool operator ==(Foo value, Foo o)
    {
        return value.name == o.name;
    }

    public static bool operator !=(Foo value, Foo o)
    {
        return !(value == o);
    }

    public static implicit operator Foo (string name)
    {
        return new Foo (name);
    }
}

class Bar
{
    public static void Main (string[] args)
    {
        var foo = new Foo (null);
        System.Console.WriteLine (foo == null);
    }
}");
        }


        /// <summary>
        /// Bug 36336 - Invalid Source Analysis on Pointer types
        /// </summary>
        [Fact]
        public void TestBug36336()
        {
            Analyze<ConditionIsAlwaysTrueOrFalseAnalyzer>(@"
            unsafe class MainClass
            {
                static extern int *GetInt();

                static void ParseInt()
                {
                    var x = GetInt();
                    if (x != null) {
                        // Do stuff
                    }
                }
            }
");
        }





    }
}

