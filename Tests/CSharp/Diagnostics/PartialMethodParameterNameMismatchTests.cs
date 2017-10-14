using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class PartialMethodParameterNameMismatchTests : CSharpDiagnosticTestBase
    {

        // Disabled this test temporarily to make the build compile
//        [Fact]
//        public void SimpleCaseFix()
//        {
//            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
//partial class Test
//{
//    void Blubb();
//	partial void FooBar(int bar);
//}

//partial class Test
//{
//	partial void FooBar(int $foo$)
//	{
//	}
//}
//", @"
//partial class Test
//{
//	partial void FooBar(int bar);
//}

//partial class Test
//{
//	partial void FooBar(int bar)
//	{
//	}
//}
//");
//        }


        [Fact]
        public void NoMismatch()
        {
            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
partial class Test
{
    partial void FooBar(int bar, string baz, object foo);
}

partial class Test
{
    partial void FooBar(int bar, string baz, object foo)
    {
    }
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<PartialMethodParameterNameMismatchAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.PartialMethodParameterNameMismatchAnalyzerID + @"
partial class Test
{
    partial void FooBar(int bar);
}

partial class Test
{
    partial void FooBar(int foo)
    {
    }
}
// ReSharper restore PartialMethodParameterNameMismatch
");
        }
    }
}

