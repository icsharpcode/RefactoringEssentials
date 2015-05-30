using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet.")]
    public class LocalVariableNotUsedTests : InspectionActionTestBase
    {

        [Test]
        public void TestUnusedVariable()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		int i;
	}
}";
            var output = @"
class TestClass {
	void TestMethod ()
	{
	}
}";
            Test<LocalVariableNotUsedAnalyzer>(input, 1, output);
            var input2 = @"
class TestClass {
	void TestMethod ()
	{
		int i, j;
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
            Test<LocalVariableNotUsedAnalyzer>(input2, 1, output2);
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
            Test<LocalVariableNotUsedAnalyzer>(input1, 0);
            Test<LocalVariableNotUsedAnalyzer>(input2, 0);
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
            Test<LocalVariableNotUsedAnalyzer>(input, 1);

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
            Test<LocalVariableNotUsedAnalyzer>(input, 0);
        }

    }
}
