using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class OptionalParameterRefOutTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

