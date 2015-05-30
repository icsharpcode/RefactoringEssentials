using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class ReplaceWithStringIsNullOrEmptyTests : InspectionActionTestBase
    {
        [Test]
        public void TestInspectorCaseNS1()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str != null && str != """")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS2()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (null != str && str != """")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorNegatedStringEmpty()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (null != str && str != string.Empty)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorStringEmpty()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (null == str || str == string.Empty)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS3()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (null != str && """" != str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS4()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str != null && str != """")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN1()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str != """" && str != null)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN2()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ("""" != str && str != null)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN3()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ("""" != str && null != str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }


        [Test]
        public void TestInspectorCaseSN4()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str != """" && null != str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS5()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str == null || str == """")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS6()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (null == str || str == """")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS7()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (null == str || """" == str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS8()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str == null || """" == str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN5()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str == """" || str == null)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN6()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ("""" == str || str == null)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN7()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ("""" == str || null == str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestInspectorCaseSN8()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (str == """" || null == str)
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [TestCase("str == null || str.Length == 0")]
        [TestCase("str == null || 0 == str.Length")]
        [TestCase("null == str || str.Length == 0")]
        [TestCase("null == str || 0 == str.Length")]
        public void TestInspectorCaseNL(string expression)
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (" + expression + @")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [TestCase("str != null && str.Length != 0")]
        [TestCase("str != null && 0 != str.Length")]
        [TestCase("str != null && str.Length > 0")]
        [TestCase("null != str && str.Length != 0")]
        [TestCase("null != str && 0 != str.Length")]
        [TestCase("null != str && str.Length > 0")]
        public void TestInspectorCaseLN(string expression)
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if (" + expression + @")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

        [Test]
        public void TestArrays()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar ()
	{
		int[] foo = new int[10];
		if (foo == null || foo.Length == 0) {
		}
	}
}");
        }

        [Test]
        public void TestInspectorCaseNS1WithParentheses()
        {
            Test<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ((str != null) && (str) != """")
			;
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty (str))
			;
	}
}");
        }

    }
}
