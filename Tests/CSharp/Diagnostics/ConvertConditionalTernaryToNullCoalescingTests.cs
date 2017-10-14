using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertConditionalTernaryToNullCoalescingTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = $str != null ? str : ""default""$;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		string c = str ?? ""default"";
	}
}");

        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = $null != str ? str : ""default""$;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		string c = str ?? ""default"";
	}
}");
        }

        [Fact]
        public void TestInspectorCase3()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = $null == str ? ""default"" : str$;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		string c = str ?? ""default"";
	}
}");
        }

        [Fact]
        public void TestInspectorCase4()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		string c = $str == null ? ""default"" : str$;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		string c = str ?? ""default"";
	}
}");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
#pragma warning disable " + CSharpDiagnosticIDs.ConvertConditionalTernaryToNullCoalescingAnalyzerID + @"
		string c = str != null ? str : ""default"";
	}
}");
        }

        [Fact]
        public void TestCastCase()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (Foo o, Bar b)
	{
		IDisposable c = $o != null ? (IDisposable)o : b$;
	}
}", @"class Foo
{
	void Bar (Foo o, Bar b)
	{
		IDisposable c = (IDisposable)o ?? b;
	}
}");
        }

        [Fact]
        public void TestCastCase2()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar (Foo o, Bar b)
	{
		IDisposable c = $o == null ? (IDisposable)b : o$;
	}
}", @"class Foo
{
	void Bar (Foo o, Bar b)
	{
		IDisposable c = (IDisposable)o ?? b;
	}
}");
        }

        [Fact]
        public void TestGenericCastCase()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar<T>(object o, T b)
	{
		T c = o != null ? (T)o : b;
	}
}");
        }

        [Fact]
        public void TestGenericCastCaseWithRefTypeConstraint()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar<T>(object o, T b) where T : class
	{
		T c = $o != null ? (T)o : b$;
	}
}", @"class Foo
{
	void Bar<T>(object o, T b) where T : class
	{
		T c = (T)o ?? b;
	}
}");
        }

        [Fact]
        public void TestGenericCastCaseAsNullable()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Foo
{
	void Bar<T>(object o, T b)
	{
		T? c = $o != null ? (T?)o : b$;
	}
}", @"class Foo
{
	void Bar<T>(object o, T b)
	{
		T? c = (T?)o ?? b;
	}
}");
        }

        [Fact]
        public void TestNullableValueCase()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Test
{
    void TestCase()
    {
		int? x = null;
		int y = $x != null ? x.Value : 0$;
    }
}", @"class Test
{
    void TestCase()
    {
		int? x = null;
		int y = x ?? 0;
    }
}");
        }

		[Fact]
		public void TestNestedExpressions()
		{
			Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"class Test
{
    void TestCase()
    {
      string s = null;
      var q = $s != null ? s : true ? ""a"" : ""b""$;
    }
}", @"class Test
{
    void TestCase()
    {
      string s = null;
      var q = s ?? (true ? ""a"" : ""b"");
    }
}");
		}

        [Fact]
        public void TestIssue264()
        {
            Analyze<ConvertConditionalTernaryToNullCoalescingAnalyzer>(@"using System.Collections.Generic;

class Test
{
    void TestCase(Test[] tests)
    {
      var output = tests == null ? new List<Test>() : new List<Test>(tests);
    }
}");
        }
    }
}

