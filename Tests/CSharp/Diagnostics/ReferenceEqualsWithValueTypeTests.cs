using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReferenceEqualsWithValueTypeTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestValueTypeCase1()
        {
            Test<ReferenceEqualsWithValueTypeAnalyzer>(@"
class TestClass
{
	void TestMethod (int i, int j)
	{
		var x = object.ReferenceEquals (i, j);
	}
}", @"
class TestClass
{
	void TestMethod (int i, int j)
	{
		var x = false;
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestValueTypeCase2()
        {
            Test<ReferenceEqualsWithValueTypeAnalyzer>(@"
class TestClass
{
	void TestMethod (int i, int j)
	{
		var x2 = ReferenceEquals (i, j);
	}
}", @"
class TestClass
{
	void TestMethod (int i, int j)
	{
		var x2 = object.Equals (i, j);
	}
}", 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNoIssue()
        {
            var input = @"
class TestClass
{
	void TestMethod<T> (object i, T j)
	{
		var x = object.ReferenceEquals (i, i);
		var x2 = object.ReferenceEquals (j, j);
	}
}";
            Test<ReferenceEqualsWithValueTypeAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<ReferenceEqualsWithValueTypeAnalyzer>(@"
class TestClass
{
	void TestMethod (int i, int j)
	{
		// ReSharper disable once ReferenceEqualsWithValueType
		var x2 = ReferenceEquals (i, j);
	}
}");
        }


    }
}
