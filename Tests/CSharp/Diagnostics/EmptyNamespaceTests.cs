using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EmptyNamespaceTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBasicCase()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
$namespace Foo
{
}$", @"");
        }

        [Test]
        public void TestCaseWithRegions()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
$namespace Foo
{
	#region Bar
	#endregion
}$", @"");
        }

        [Test]
        public void TestCaseWithUsing()
        {
            Analyze<EmptyNamespaceAnalyzer>(@"
$namespace Foo
{
	using System;
}$", @"");
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

