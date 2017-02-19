using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class SealedMemberInSealedClassTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBasicCase()
        {
            Analyze<SealedMemberInSealedClassAnalyzer>(@"
sealed class Foo
{
	public $sealed$ override string ToString()
	{
		return ""''"";
	}
}
", @"
sealed class Foo
{
	public override string ToString()
	{
		return ""''"";
	}
}
");
        }

        [Fact]
        public void TestFieldDeclaration()
        {
            Analyze<SealedMemberInSealedClassAnalyzer>(@"
public sealed class Foo
{
	private int field;
}
");
        }

        [Fact]
        public void TestValid()
        {
            Analyze<SealedMemberInSealedClassAnalyzer>(@"
class Foo
{
	public sealed override string ToString()
	{
		return ""''"";
	}
}
");
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<SealedMemberInSealedClassAnalyzer>(@"
sealed class Foo
{
#pragma warning disable " + CSharpDiagnosticIDs.SealedMemberInSealedClassAnalyzerID + @"
	public sealed override string ToString()
	{
		return ""''"";
	}
}
");
        }
    }
}

