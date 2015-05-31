using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class OptionalParameterRefOutTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestRef()
        {
            Analyze<OptionalParameterRefOutAnalyzer>(@"
using System.Runtime.InteropServices;
class Bar
{
	public void Foo($[Optional] ref int test$)
	{
	}
}");
        }

        [Test]
        public void TestOut()
        {
            Analyze<OptionalParameterRefOutAnalyzer>(@"
using System.Runtime.InteropServices;
class Bar
{
	public void Foo($[Optional] out int test$)
	{
	}
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<OptionalParameterRefOutAnalyzer>(@"
using System.Runtime.InteropServices;
class Bar
{
#pragma warning disable " + CSharpDiagnosticIDs.OptionalParameterRefOutAnalyzerID + @"
	public void Foo([Optional] ref int test)
	{
	}
}
");
        }

    }
}

