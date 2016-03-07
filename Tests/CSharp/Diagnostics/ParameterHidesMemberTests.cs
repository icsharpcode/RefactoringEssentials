using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ParameterHidesMemberTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestField()
        {
            var input = @"
class TestClass
{
	int i;
	void TestMethod (int $i$, int j)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
	int i;
#pragma warning disable " + CSharpDiagnosticIDs.ParameterHidesMemberAnalyzerID + @"
	void TestMethod (int i, int j)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestMethod()
        {
            var input = @"
class TestClass
{
	void TestMethod2 ()
	{ }
	void TestMethod (int $TestMethod2$)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestConstructor()
        {
            var input = @"
class TestClass
{
	int i;
	public TestClass(int i)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestStatic()
        {
            var input = @"
class TestClass
{
	static int i;
	static void TestMethod2 (int $i$)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestStaticNoIssue()
        {
            var input = @"
class TestClass
{
	static int i;
	int j;
	void TestMethod (int $i$)
	{
	}
	static void TestMethod2 (int j)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestAccessiblePrivate()
        {
            var input = @"
class TestClass
{
	int i;

	void Method (int $i$)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestAccessiblePrivateDueToTypeNesting()
        {
            var input = @"
class RootClass
{
	int i;

	class NestedClass : RootClass
	{
		// Issue 1
		void Method (int $i$) {}

		class NestedNestedClass : NestedClass
		{
			// Issue 2
			void OtherMethod (int $i$) {}
		}
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestInternalAccessibility()
        {
            var input = @"
class BaseClass
{
	internal int i;
}
class TestClass : BaseClass
{
	void Method (int $i$)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestInaccessiblePrivate()
        {
            var input = @"
class BaseClass
{
	private int i;
}
class TestClass : BaseClass
{
	void Method (int i)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestIgnorePublicMethods()
        {
            var input = @"
class TestClass
{
	private int i;

	public void SetI (int i) { this.i = i; }
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }


        [Test]
        public void TestIgnoreAbstractMethods()
        {
            var input = @"
abstract class TestClass
{
	private int i;

	public abstract void Method (int i);
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestIgnoreOverriddenMethods()
        {
            var input = @"
class TestClass
{
	private int i;

	protected override void Method (int i)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestIgnoreInterfaceImplementations()
        {
            var input = @"
interface ITest {
	void Method (int i);
}

class TestClass : ITest 
{
	private int i;

	void ITest.Method (int i)
	{
	}
}";
            Analyze<ParameterHidesMemberAnalyzer>(input);
        }
    }
}
