using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class LongLiteralEndingLowerLTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestNormal()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public long x = $3l$;
}", @"
class Test
{
	public long x = 3L;
}");
        }

        [Test]
        public void TestDisabledForUnsignedFirst()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public ulong x = 3ul;
}");
        }

        [Test]
        public void TestUnsigned()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public ulong x = $3lu$;
}", @"
class Test
{
	public ulong x = 3LU;
}");
        }

        [Test]
        public void TestDisabledForUppercase()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public long x = 3L;
}");
        }

        [Test]
        public void TestDisabledForString()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public string x = ""l"";
}");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
#pragma warning disable " + CSharpDiagnosticIDs.LongLiteralEndingLowerLAnalyzerID + @"
	public long x = 3l;
}");
        }
    }
}