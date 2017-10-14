using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ReplaceWithStringIsNullOrEmptyTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestMemberAccessCase()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(
                @"
class Foo
{
	void Bar ()
	{
        var str = new { Name = ""John"" };
		if ($str.Name != null && str.Name != """"$);
	}
}", @"
class Foo
{
	void Bar ()
	{
        var str = new { Name = ""John"" };
		if (!string.IsNullOrEmpty(str.Name));
	}
}");
        }

        [Fact]
        public void TestToStringCase()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(
                @"
using System.Text;
class Foo
{
	void Bar (StringBuilder str)
	{
		if ($str.ToString() != null && str.ToString() != """"$);
	}
}", @"
using System.Text;
class Foo
{
	void Bar (StringBuilder str)
	{
		if (!string.IsNullOrEmpty(str.ToString()));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS1()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str != null && str != """"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS2()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($null != str && str != """"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorNegatedStringEmpty()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($null != str && str != string.Empty$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorStringEmpty()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($null == str || str == string.Empty$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorStringEmptyAlt()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str == string.Empty || null == str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS3()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($null != str && """" != str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS4()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str != null && str != """"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN1()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str != """" && str != null$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN2()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($"""" != str && str != null$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN3()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($"""" != str && null != str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }


        [Fact]
        public void TestInspectorCaseSN4()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str != """" && null != str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS5()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str == null || str == """"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS6()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($null == str || str == """"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS7()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($null == str || """" == str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS8()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str == null || """" == str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN5()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str == """" || str == null$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN6()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($"""" == str || str == null$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN7()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($"""" == str || null == str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
        public void TestInspectorCaseSN8()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($str == """" || null == str$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [InlineData("str == null || str.Length == 0")]
        [InlineData("str == null || 0 == str.Length")]
        [InlineData("null == str || str.Length == 0")]
        [InlineData("null == str || 0 == str.Length")]
        public void TestInspectorCaseNL(string expression)
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($" + expression + @"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (string.IsNullOrEmpty(str));
	}
}");
        }

        [InlineData("str != null && str.Length != 0")]
        [InlineData("str != null && 0 != str.Length")]
        [InlineData("str != null && str.Length > 0")]
        [InlineData("null != str && str.Length != 0")]
        [InlineData("null != str && 0 != str.Length")]
        [InlineData("null != str && str.Length > 0")]
        public void TestInspectorCaseLN(string expression)
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($" + expression + @"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

        [Fact]
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

        [Fact]
        public void TestLists()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"
using System.Collections.Generic;

class Foo
{
	void Bar ()
	{
		var foo = new List<string>();
		if (foo == null || foo.Length == 0) {
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCaseNS1WithParentheses()
        {
            Analyze<ReplaceWithStringIsNullOrEmptyAnalyzer>(@"class Foo
{
	void Bar (string str)
	{
		if ($(str != null) && (str) != """"$);
	}
}", @"class Foo
{
	void Bar (string str)
	{
		if (!string.IsNullOrEmpty(str));
	}
}");
        }

    }
}
