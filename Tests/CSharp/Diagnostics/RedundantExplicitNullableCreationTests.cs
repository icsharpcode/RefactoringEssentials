using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantExplicitNullableCreationTests : InspectionActionTestBase
    {
        [Test]
        public void TestVariableCreation()
        {
            Test<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
	void Test()
	{
		int? i = new int?(5);
	}
}
", @"
class FooBar
{
	void Test()
	{
		int? i = 5;
	}
}
");
        }

        [Test]
        public void TestLongForm()
        {
            Test<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
	void Test()
	{
		int? i = new System.Nullable<int>(5);
	}
}
", @"
class FooBar
{
	void Test()
	{
		int? i = 5;
	}
}
");
        }

        [Test]
        public void TestInvalid()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
	void Test()
	{
		var i = new int?(5);
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<RedundantExplicitNullableCreationAnalyzer>(@"
class FooBar
{
	void Test()
	{
		// ReSharper disable once RedundantExplicitNullableCreation
		int? i = new int?(5);
	}
}
");
        }
    }
}

