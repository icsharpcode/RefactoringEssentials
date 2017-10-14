using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class LocalVariableHidesMemberTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestField()
        {
            const string input = @"
class TestClass
{
	int i;
	void TestMethod ()
	{
		int $i$, j;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
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

        [Fact]
        public void TestMethod()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		int $TestMethod$;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
        public void TestForeach()
        {
            var input = @"
class TestClass
{
	int i;
	void TestMethod ()
	{
		int[] array = new int [10];
		foreach (var $i$ in array) ;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
        public void TestStatic()
        {
            var input = @"
class TestClass
{
	static int i;
	static void TestMethod2 ()
	{
		int $i$;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
        public void TestStaticNoIssue()
        {
            var input = @"
class TestClass
{
	static int i;
	int j;
	void TestMethod ()
	{
		int $i$;
	}
	static void TestMethod2 ()
	{
		int j;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
        public void TestAccessiblePrivate()
        {
            var input = @"
class TestClass
{
	int i;

	void Method ()
	{
		int $i$ = 0;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
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
			int $i$ = 0;
		}

		class NestedNestedClass : NestedClass
		{
			// Issue 2
			void OtherMethod ()
			{
				int $i$ = 0;
			}
		}
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
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
		int $i$ = 0;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
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
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
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
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
        public void TestClashWithTypeName1()
        {
            var input = @"
class i
{
}
class TestClass
{
	void Method ()
	{
		int i = 0;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }

        [Fact]
        public void TestClashWithTypeName2()
        {
            var input = @"
delegate int i();

class TestClass
{
	void Method ()
	{
		int i = 0;
	}
}";
            Analyze<LocalVariableHidesMemberAnalyzer>(input);
        }
    }
}
