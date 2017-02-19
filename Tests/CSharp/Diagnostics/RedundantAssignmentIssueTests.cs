using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantAssignmentTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVariableInitializerNotUsed()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		int i = 1;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVariableInitializerNotUsedVar()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var i = 1;
	}
}";
            var output = @"
class TestClass
{
	void TestMethod ()
	{
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVariableAssignmentNotUsed()
        {
            var input = @"
class TestClass
{
	int TestMethod ()
	{
		int i = 1;
		int j = i;
		i = 2;
		return j;
	}
}";
            var output = @"
class TestClass
{
	int TestMethod ()
	{
		int i = 1;
		int j = i;
		return j;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestParameterAssignmentNotUsed()
        {
            var input = @"
class TestClass
{
	int TestMethod (int i)
	{
		int j = i;
		i = 2;
		return j;
	}
}";
            var output = @"
class TestClass
{
	int TestMethod (int i)
	{
		int j = i;
		return j;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInExpression()
        {
            var input = @"
class TestClass
{
	int TestMethod (int i)
	{
		int j = i = 2;
		return j;
	}
}";
            var output = @"
class TestClass
{
	int TestMethod (int i)
	{
		int j = 2;
		return j;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOutArgument()
        {
            var input = @"
class TestClass
{
	void Test (out int i)
	{
		i = 0;
	}
	int TestMethod ()
	{
		int i = 2;
		Test (out i);
		return i;
	}
}";
            var output = @"
class TestClass
{
	void Test (out int i)
	{
		i = 0;
	}
	int TestMethod ()
	{
		int i;
		Test (out i);
		return i;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOutArgument2()
        {
            var input = @"
class TestClass
{
	void Test (out int i)
	{
		i = 0;
	}
	int TestMethod ()
	{
		int i;
		Test (out i);
		i = 2;
		return i;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRefArgument()
        {
            var input = @"
class TestClass
{
	void Test (ref int i)
	{
		i = 0;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentOperator()
        {
            var input = @"
class TestClass
{
	int TestMethod ()
	{
		int i = 1;
		i += 2;
		return i;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIf()
        {
            var input = @"
class TestClass
{
	int TestMethod (int j)
	{
		int i = 1;
		if (j > 0) {
			i += 2;
		} else {
		}
		return i;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestConditionalExpression()
        {
            var input = @"
class TestClass
{
	int TestMethod (int j)
	{
		int i = 1;
		return j > 0 ? i : 0;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLoop()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var x = 0;
		for (int i = 0; i < 10; i++) {
			if (i > 5) {
				x++;
			} else {
				x = 2;
			}
		}
		if (x > 1) ;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestForeach()
        {
            var input = @"
class TestClass
{
	void TestMethod (int[] array)
	{
		foreach (int j in array) {
			bool x = false;
			foreach (int k in array)
				foreach (int i in array)
					if (i > 5) x = true;
			if (x) break;
		}
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInTryCatch()
        {
            var input = @"using System;
class TestClass
{
	void TestMethod ()
	{
		var a = new TestClass ();
		try {
			a = null;
		} catch (Exception) {
			if (a != null) {
				a.TestMethod ();
			}
		}
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInTryCatchFinally()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		var a = new TestClass ();
		try {
			a = null;
		} finally {
			if (a != null) {
				a.TestMethod ();
			}
		}
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInCatch()
        {
            var input = @"using System;
class TestClass
{
    void Test(TestClass a) { }

	void TestMethod ()
	{
		var a = new TestClass ();
		try {
		} catch (Exception) {
			a = null;
		}
        Test (a);
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentBeforeTry()
        {
            var input = @"using System;
class TestClass
{
    void Test(TestClass a) { }

	void TestMethod ()
	{
		var a = null;
		try {
			a = new TestClass ();
		} catch (Exception) {
		}
        Test (a);
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInUsing()
        {
            var input = @"using System;
class TestClass
{
	void TestMethod ()
	{
		using (var tc = new TestClass ()) {
			// nothing
		}
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentWithFunction()
        {
            var input = @"using System;
class TestClass
{
	TestClass  Func () { return null; }
	void TestMethod ()
	{
		var a = Func ();
	}
}";
            var output = @"using System;
class TestClass
{
	TestClass  Func () { return null; }
	void TestMethod ()
	{
		Func ();
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentWithFunctionUsedLater()
        {
            var input = @"using System;
class TestClass
{
	TestClass  Func () { return null; }
	void TestMethod ()
	{
		var a = Func ();
        a = 2;
	}
}";
            var output = @"using System;
class TestClass
{
	TestClass  Func () { return null; }
	void TestMethod ()
	{
		TestClass a;
		Func ();
        a = 2;
	}
}";
            Test<RedundantAssignmentAnalyzer>(input, 2, output, 0);
        }


        /// <summary>
        /// Bug 11795 - Use of regex in linq statement not being recognized. 
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug11795()
        {
            Analyze<RedundantAssignmentAnalyzer>(@"
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

public class Test
{
	public void Demo ()
	{
		Regex pattern = new Regex (@""^.*\.(jpg|png)$"", RegexOptions.IgnoreCase);
		string path = Path.Combine (""/"", ""speakers"");

		Console.WriteLine (
			from file in Directory.GetFiles (path)
			where pattern.IsMatch (file)
			select file
			);
	}

}");

        }


        /// <summary>
        /// Bug 14929 - Assignment greyed out (meaning "redundant") when it should not be
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug14929()
        {
            Analyze<RedundantAssignmentAnalyzer>(@"
using system;

public class Test
{
	public void Demo ()
	{
		bool save = true;
		try {
			throw new Exception ();
		} catch (Exception) {
			save = false;
			throw;
		} finally {
			System.Console.WriteLine (save);
		}
	}

}");

        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMultipleVariableInitializers()
        {
            Test<RedundantAssignmentAnalyzer>(@"using System;
public class MyClass
{
	public static void Main ()
	{
		string outputFile = null, inputFile = null;
		Console.WriteLine (outputFile);
	}
}
", 1, @"using System;
public class MyClass
{
	public static void Main ()
	{
		string outputFile = null, inputFile;
		Console.WriteLine (outputFile);
	}
}
", 0);
        }
    }
}