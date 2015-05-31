using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EnumUnderlyingTypeIsIntTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
        public void TestDisable()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
#pragma warning disable " + CSharpDiagnosticIDs.EnumUnderlyingTypeIsIntAnalyzerID + @"
public enum Foo : int
{
    Bar
}");
        }


        [Test]
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

        [Test]
        public void TestDisabledForNoUnderlyingType()
        {
            Analyze<EnumUnderlyingTypeIsIntAnalyzer>(@"
public enum Foo
{
    Bar
}");
        }

        [Test]
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