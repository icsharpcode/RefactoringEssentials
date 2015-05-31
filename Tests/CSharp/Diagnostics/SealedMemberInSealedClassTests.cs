using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class SealedMemberInSealedClassTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
        public void TestFieldDeclaration()
        {
            Analyze<SealedMemberInSealedClassAnalyzer>(@"
public sealed class Foo
{
	private int field;
}
");
        }

        [Test]
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


        [Test]
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

