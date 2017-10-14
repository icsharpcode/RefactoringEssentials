using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
	public class EmptyNamespaceTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBasicCase()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
$namespace Foo
{
}$", @"");
        }

        [Fact]
        public void TestCaseWithRegions()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
$namespace Foo
{
	#region Bar
	#endregion
}$", @"");
        }

        [Fact]
        public void TestCaseWithUsing()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
$namespace Foo
{
	using System;
}$", @"");
        }

        [Fact]
        public void TestCaseWithNesting()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
namespace Foo
{
	$namespace Bar
	{
	}$
}", @"
namespace Foo
{
}");
        }

        [Fact]
        public void TestDisabledForNonEmpty()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
namespace Foo
{
	class Bar
	{
	}
}");
        }

        [Fact]
        public void TestDisabledForRegionsWithClasses()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
namespace Foo
{
	#region Baz
		class Bar
		{
		}
	#endregion
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.EmptyNamespaceAnalyzerID + @"
namespace Foo
{
}");
        }
    }
}

