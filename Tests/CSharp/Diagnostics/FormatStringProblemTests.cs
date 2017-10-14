using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class FormatStringProblemTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TooFewArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""${0}$"");
	}
}");
        }


        [Fact]
        public void SupportsFixedArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		Bar(""{0}"", 1);
	}

	void Bar(string format, string arg0)
	{
	}
}");
        }

        [Fact]
        public void IgnoresCallWithUnknownNumberOfArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	string Bar(params object[] args)
	{
		return string.Format(""{1}"", args);
	}
}");
        }

        [Fact]
        public void FormatItemIndexOutOfRangeOfArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""${1}$"", $1$);
	}
}");
        }

        [Fact]
        public void FormatItemIndexOutOfRangeOfArguments_ExplicitArrayCreation()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""${1}$"", $new object[] { 1 }$);
	}
}");
        }

        [Fact]
        public void FormatItemMissingEndBrace()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""Text text text ${0$ text text text"", 1);
	}
}");
        }

        [Fact]
        public void UnescapedLeftBrace()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""a ${$ a"", $1$);
	}
}");
        }

        [Fact]
        public void IgnoresStringWithGoodArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""{0}"", ""arg0"");
	}
}");
        }

        [Fact]
        public void IgnoresStringWithGoodArguments_ExplicitArrayCreation()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""{0} {1}"", new object[] { ""arg0"", ""arg1"" });
	}
}");
        }

        [Fact]
        public void IgnoresNonFormattingCall()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string lower = string.ToLower(""{0}"");
	}
}");
        }

        [Fact]
        public void HandlesCallsWithExtraArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		Foo(1);
	}
}");
        }


        [Fact]
        public void TooManyArguments()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""{0}"", 1, $2$);
	}
}");
        }

        [Fact]
        public void UnreferencedArgument()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		string.Format(""{0} {2}"", 1, $2$, 3);
	}
}");
        }

        /// <summary>
        /// Bug 14405 - Incorrect "argument not used in string format" warning
        /// </summary>
        [Fact]
        public void TestBug14405()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
using System;
class TestClass
{
	void Foo()
	{
		DateTime.ParseExact(""expiresString"", ""s"", System.Globalization.CultureInfo.InvariantCulture);
	}
}");
        }
        [Fact]
        public void TestDisable()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
#pragma warning disable " + CSharpDiagnosticIDs.FormatStringProblemAnalyzerID + @"
		string.Format(""{0}"", 1, 2);
	}
}");
        }

        /// <summary>
        /// Bug 15867 - Wrong Context for string formatting
        /// </summary>
        [Fact]
        public void TestBug15867()
        {
            Analyze<FormatStringProblemAnalyzer>(@"
class TestClass
{
	void Foo()
	{
		double d = 1;
		d.ToString(""G29"", System.Globalization.CultureInfo.InvariantCulture);
	}
}");
        }
    }
}

