using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class PartialTypeWithSinglePartTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestRedundantModifier()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(
@"$partial$ class TestClass
{
}", @"class TestClass
{
}");
        }

        [Fact]
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

        [Fact]
        public void TestDisable()
        {
            Analyze<PartialTypeWithSinglePartAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.PartialTypeWithSinglePartDiagnosticID + @"
partial class TestClass
{
}");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
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