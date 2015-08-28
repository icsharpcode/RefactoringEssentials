using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class UseArrayCreationExpressionTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestTypeOfIsAssignableFrom()
        {
            Analyze<UseArrayCreationExpressionAnalyzer>(@"
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

        [Test]
        public void MultiDim()
        {
            Analyze<UseArrayCreationExpressionAnalyzer>(@"
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

        [Test]
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

