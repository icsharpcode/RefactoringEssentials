using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class CS1717AssignmentMadeToSameVariableTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestVariable()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		int a = 0;
		a = a;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
		int a = 0;
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 1, output);
        }

        [Ignore("Parser/AST generation error - WarningList is empty")]
        [Test]
        public void TestPragmaSuppression()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
        int a = 0;
#pragma warning disable 1717
        a = a;
#pragma warning restore 1717
	}
}";
            Analyze<CS1717AssignmentMadeToSameVariableAnalyzer>(input);
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
        int a = 0;
// ReSharper disable once CSharpWarnings::CS1717
        a = a;
	}
}";
            Analyze<CS1717AssignmentMadeToSameVariableAnalyzer>(input);
        }



        [Test]
        public void TestParameter()
        {
            var input = @"
class TestClass
{
	void TestMethod (int a)
	{
		a = a;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod (int a)
	{
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestField()
        {
            var input = @"
class TestClass
{
	int a;
	void TestMethod ()
	{
		a = a;
		this.a = this.a;
		this.a = a;
	}
}";
            var output = @"
class TestClass
{
	int a;
	void TestMethod ()
	{
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 3, output);
        }

        [Test]
        public void TestFix()
        {
            var input = @"
class TestClass
{
	void Test (int i) { }
	void TestMethod ()
	{
		int a = 0;
		a = a;
		Test (a = a);
	}
}";
            var output = @"
class TestClass
{
	void Test (int i) { }
	void TestMethod ()
	{
		int a = 0;
		Test (a);
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 2, output);
        }

        [Test]
        public void TestNoIssue()
        {
            var input = @"
class TestClass
{
	int a;
	int b;

	public int Prop { get; set; }

	void TestMethod (int a)
	{
		a = b;
		this.a = b;
		this.a = a;
		Prop = Prop;
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 0);
        }

        [Test]
        public void IgnoresAssignmentWithDifferentRootObjects()
        {
            var input = @"
class TestClass
{
	int a;

	void TestMethod (TestClass tc)
	{
		a = tc.a;
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 0);
        }

        [Test]
        public void NestedFieldAccess()
        {
            var input = @"
class TestClass
{
	int a;

	TestClass nested;

	void TestMethod ()
	{
		nested.nested.a = nested.nested.a;
	}
}";
            var output = @"
class TestClass
{
	int a;

	TestClass nested;

	void TestMethod ()
	{
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 1, output);
        }

        [Test]
        public void NestedPropertyAccess()
        {
            var input = @"
class TestClass
{
	int a;

	TestClass nested { get; set; }

	void TestMethod ()
	{
		nested.nested.a = nested.nested.a;
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 0);
        }

        [Test]
        public void TestNoIssueWithCompoundOperator()
        {
            var input = @"
class TestClass
{
	void TestMethod (int a)
	{
		a += a;
	}
}";
            Test<CS1717AssignmentMadeToSameVariableAnalyzer>(input, 0);
        }
    }
}
