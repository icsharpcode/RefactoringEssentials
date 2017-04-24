using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class NameOfSuggestionTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestArgumentNullException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentNullException($""foo""$, ""bar"");
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentNullException(nameof(foo), ""bar"");
	}
}");
        }

        [Fact]
        public void TestArgumentException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", $""foo""$);
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", nameof(foo));
	}
}");
        }

        [Fact]
        public void TestArgumentOutOfRangeExceptionSwap()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException($""foo""$, ""foo"");
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException(nameof(foo), ""foo"");
	}
}", 0);
        }

        [Fact]
        public void TestFullyQualifiedArgumentException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new System.ArgumentException(""bar"", $""foo""$);
	}
}", @"
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new System.ArgumentException(""bar"", nameof(foo));
	}
}");
        }

        [Fact]
        public void TestFullyQualifiedWithAliasArgumentException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
using ss = System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ss::ArgumentException(""bar"", $""foo""$);
	}
}", @"
using ss = System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ss::ArgumentException(""bar"", nameof(foo));
	}
}");
        }
    }
}

