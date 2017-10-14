using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EnumUnderlyingTypeIsIntTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestCase()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
public enum Foo $: int$
{
    Bar
}", @"
public enum Foo
{
    Bar
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.EnumUnderlyingTypeIsIntAnalyzerID + @"
public enum Foo : int
{
    Bar
}");
        }


        [Fact]
        public void TestNestedCase()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
class Outer
{
    public enum Foo $: int$
    {
        Bar
    }
}", @"
class Outer
{
    public enum Foo
    {
        Bar
    }
}");
        }

        [Fact]
        public void TestDisabledForNoUnderlyingType()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
public enum Foo
{
    Bar
}");
        }

        [Fact]
        public void TestDisabledForOtherTypes()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
public enum Foo : byte
{
    Bar
}");
        }
    }
}