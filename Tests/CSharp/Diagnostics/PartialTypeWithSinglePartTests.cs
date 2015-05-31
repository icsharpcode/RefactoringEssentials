using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class PartialTypeWithSinglePartTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestRedundantModifier()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(
@"$partial$ class TestClass
{
}", @"class TestClass
{
}");
        }

        [Test]
        public void TestNecessaryModifier()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>((string)@"
partial class TestClass
{
}
partial class TestClass
{
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID + @"
partial class TestClass
{
}");
        }

        [Test]
        public void TestRedundantNestedPartial()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(@"
partial class TestClass
{
	$partial$ class Nested
	{
	}
}
partial class TestClass
{
}", @"
partial class TestClass
{
	class Nested
	{
	}
}
partial class TestClass
{
}");
        }

        [Test]
        public void TestRedundantNestedPartialInNonPartialOuterClass()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(@"
class TestClass
{
	$partial$ class Nested
	{
	}
}", @"
class TestClass
{
	class Nested
	{
	}
}");
        }

        [Test]
        public void TestRedundantNestedPartialDisable()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID + @"
partial class TestClass
{
	#pragma warning restore " + CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID + @"
	$partial$ class Nested
	{
	}
}
", @"
#pragma warning disable " + CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID + @"
partial class TestClass
{
	#pragma warning restore " + CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID + @"
	class Nested
	{
	}
}
");
        }


        [Test]
        public void TestNeededNestedPartial()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(@"
partial class TestClass
{
	partial class Nested
	{
	}
}
partial class TestClass
{
	partial class Nested
	{
	}
}");
        }


    }
}