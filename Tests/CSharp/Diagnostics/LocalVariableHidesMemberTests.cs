using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class LocalVariableHidesMemberTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestField()
        {
            const string input = @"
class TestClass
{
	int i;
	void TestMethod ()
	{
		int i, j;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 1);
        }

        public void TestDisable()
        {
            var input = @"class TestClass
{
    int i;
    void TestMethod()
    {
// ReSharper disable once LocalVariableHidesMember
        int i, j;
    }
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Test]
        public void TestMethod()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		int TestMethod;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 1);
        }

        [Test]
        public void TestForeach()
        {
            var input = @"
class TestClass
{
	int i;
	void TestMethod ()
	{
		int[] array = new int [10];
		foreach (var i in array) ;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 1);
        }

        [Test]
        public void TestStatic()
        {
            var input = @"
class TestClass
{
	static int i;
	static void TestMethod2 ()
	{
		int i;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 1);
        }

        [Test]
        public void TestStaticNoIssue()
        {
            var input = @"
class TestClass
{
	static int i;
	int j;
	void TestMethod ()
	{
		int i;
	}
	static void TestMethod2 ()
	{
		int j;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 0);
        }

        [Test]
        public void TestAccessiblePrivate()
        {
            var input = @"
class TestClass
{
	int i;

	void Method ()
	{
		int i = 0;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 1);
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
		void Method ()
		{
			int i = 0;
		}

		class NestedNestedClass : NestedClass
		{
			// Issue 2
			void OtherMethod ()
			{
				int i = 0;
			}
		}
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 2);
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
	void Method ()
	{
		int i = 0;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 1);
        }

        [Test]
        public void TestInaccessiblePrivate()
        {
            var input = @"
class BaseClass
{
	int i;
}
class TestClass : BaseClass
{
	void Method ()
	{
		int i = 0;
	}
}";
            Test<LocalVariableHidesMemberAnalyzer>(input, 0);
        }

        [Test]
        public void SuppressIssueIfVariableInitializedFromField()
        {
            var input = @"
class TestClass
{
	int i;
	
	void Method ()
	{
		int i = this.i;
	}
}";
            // Given the initializer, member hiding is obviously intended in this case;
            // so we suppress the warning.
            Test<LocalVariableHidesMemberAnalyzer>(input, 0);
        }
    }
}
