using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class UseArrayCreationExpressionTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTypeOfIsAssignableFrom()
        {
            Test<UseArrayCreationExpressionAnalyzer>(@"
class Test
{
	void Foo()
	{
		System.Array.CreateInstance(typeof(int), 10);
	}
}
", @"
class Test
{
	void Foo()
	{
		new int[10];
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void MultiDim()
        {
            Test<UseArrayCreationExpressionAnalyzer>(@"
class Test
{
	void Foo(int i)
	{
		System.Array.CreateInstance(typeof(int), 10, 20, i);
	}
}
", @"
class Test
{
	void Foo(int i)
	{
		new int[10, 20, i];
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<UseArrayCreationExpressionAnalyzer>(@"
class Test
{
	void Foo()
	{
		// ReSharper disable once UseArrayCreationExpression
		System.Array.CreateInstance(typeof(int), 10);
	}
}
");
        }

    }
}

