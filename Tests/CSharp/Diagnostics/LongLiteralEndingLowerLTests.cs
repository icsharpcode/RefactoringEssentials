using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class LongLiteralEndingLowerLTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
        public void TestDisabledForUnsignedFirst()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public ulong x = 3ul;
}");
        }

        [Fact]
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

        [Fact]
        public void TestDisabledForUppercase()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public long x = 3L;
}");
        }

        [Fact]
        public void TestDisabledForString()
        {
            Analyze<LongLiteralEndingLowerLAnalyzer>(@"
class Test
{
	public string x = ""l"";
}");
        }

        [Fact]
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