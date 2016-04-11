using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConvertConditionalTernaryToNullCoalescingTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

		[Test]
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
    }
}

