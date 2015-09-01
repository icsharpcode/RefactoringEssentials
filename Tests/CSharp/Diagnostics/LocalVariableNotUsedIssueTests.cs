using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class LocalVariableNotUsedTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnusedVariable()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		int $i$;
	}
}";
            var output = @"
class TestClass {
	void TestMethod ()
	{
	}
}";
            Analyze<LocalVariableNotUsedAnalyzer>(input, output);
        }

        [Test]
        [Ignore("Support for multiple variables not implemented yet. Reactivate when finished.")]
        public void TestUnusedVariable2()
        {
            var input2 = @"
class TestClass {
	void TestMethod ()
	{
		int $i$, j;
		j = 1;
	}
}";
            var output2 = @"
class TestClass {
	void TestMethod ()
	{
		int j;
		j = 1;
	}
}";
            Analyze<LocalVariableNotUsedAnalyzer>(input2, output2);
        }

        [Test]
        public void TestUsedVariable()
        {
            var input1 = @"
class TestClass {
	void TestMethod ()
	{
		int i = 0;
	}
}";
            var input2 = @"
class TestClass {
	void TestMethod ()
	{
		int i;
		i = 0;
	}
}";
            Analyze<LocalVariableNotUsedAnalyzer>(input1);
            Analyze<LocalVariableNotUsedAnalyzer>(input2);
        }

        [Test]
        public void TestUnusedForeachVariable()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		var array = new int[10];
		foreach (var i in array) {
		}
	}
}";
            Analyze<LocalVariableNotUsedAnalyzer>(input);
        }

        [Test]
        public void TestUsedForeachVariable()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		var array = new int[10];
		int j = 0;
		foreach (var i in array) {
			j += i;
		}
	}
}";
            Analyze<LocalVariableNotUsedAnalyzer>(input);
        }
    }
}