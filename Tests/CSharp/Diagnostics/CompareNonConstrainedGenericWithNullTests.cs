using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class CompareNonConstrainedGenericWithNullTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestLocal()
        {
            Analyze<CompareNonConstrainedGenericWithNullAnalyzer>(@"public class Bar
{
	public void Foo<T> (T t)
	{
		if (t == $null$) {
		}
	}
}"
/*, @"public class Bar
{
	public void Foo<T> (T t)
	{
		if (t == default(T)) {
		}
	}
}"*/);
        }

        [Fact]
        public void TestField()
        {
            Analyze<CompareNonConstrainedGenericWithNullAnalyzer>(@"public class Bar<T>
{
	T t;
	public void Foo ()
	{
		if (t == $null$) {
		}
	}
}"
/*, @"public class Bar<T>
{
	T t;
	public void Foo ()
	{
		if (t == default(T)) {
		}
	}
}"*/);
        }

        [Fact]
        public void TestInvalid()
        {
            Analyze<CompareNonConstrainedGenericWithNullAnalyzer>(@"public class Bar
{
	public void Foo<T> (T t) where T : class
	{
		if (t == null) {
		}
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<CompareNonConstrainedGenericWithNullAnalyzer>(@"public class Bar
{
	public void Foo<T> (T t)
	{
#pragma warning disable " + CSharpDiagnosticIDs.CompareNonConstrainedGenericWithNullAnalyzerID + @"
		if (t == null) {
		}
	}
}");
        }
    }
}

