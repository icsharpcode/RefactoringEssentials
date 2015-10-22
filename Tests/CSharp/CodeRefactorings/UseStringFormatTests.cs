using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class UseStringFormatTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        [Description("String concatenation alone should not be replaced by concatenated string.")]
        public void TestSimpleStringConcatenation()
        {
            Test<UseStringFormatAction>(
                @"var s = ""Hello"" + …""World!"";",
                @"var s = ""HelloWorld!"";");
        }

        [Test]
        [Description("String concatenation alone should not be replaced by concatenated string.")]
        public void TestSimpleVerbatimStringConcatenation()
        {
            Test<UseStringFormatAction>(
                @"var s = @""Hello"" + …@""World!"";",
                @"var s = @""HelloWorld!"";");
        }

        [Test]
        [Description("Concatenation between verbatim and non-verbatim strings is messy, so leave it alone.")]
        public void TestWrongContextForVerbatimAndNonVerbatimStrings()
        {
            TestWrongContext<UseStringFormatAction>(@"var s = @""Hello"" + …""World!"";");
        }

        [Test]
        [Description("Ensure action is not applied to addition syntax without string literals.")]
        public void TestWrongContextForAddition()
        {
            TestWrongContext<UseStringFormatAction>(@"var s = 1 + …2;");
        }

        [Test]
        [Description("Concatenation with member access to identifiers should be modified to use string.Format()")]
        public void TestIdentifiersWithMemberAccess()
        {
            Test<UseStringFormatAction>(
                @"class TestClass
                {
                    void TestMethod()
                    {
                        string str = ""Hello \x143 "" + …a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");
                    }
                }",
                @"class TestClass
                {
                    void TestMethod()
                    {
                        string str = string.Format(""Hello \x143 {0} world! {1}"", a.Foo(""asdf""), a.Bar(""jkl;""));
                    }
                }");
        }


        [Test]
        [Description("Concatenation between verbatim strings and expressions is replaced with string.Format()")]
        public void TestVerbatimStringConcatenation()
        {
            Test<UseStringFormatAction>(
                @"class TestClass
                {
                    void TestMethod()
                    {
                        string str = @""Hello
                            "" + …a.Blah(""asdf"") + @""
                            world!"";
                    }
                }",
                @"class TestClass
                {
                    void TestMethod()
                    {
                        string str = string.Format(@""Hello
                            {0}
                            world!"", a.Blah(""asdf""));
                    }
                }");
        }

        [Test]
        public void Test()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		string str = 1 + $2 + ""test"" + 1 + ""test"" + 1.1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		string str = string.Format(""{0}test{1}test{2}"", 1 + 2, 1, 1.1);
	}
}");
        }

        [Test]
        public void TestVerbatim()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		string str = $@""
test "" + 1;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		string str = string.Format(@""
test {0}"", 1);
	}
}");
        }

        [Test]
        public void TestRepeatedObject()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = …""test"" + i + ""test"" + i;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = string.Format(""test{0}test{0}"", i);
	}
}");
        }

        [Test]
        public void TestFormatString()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = …""A test number: "" + i.ToString(""N2"");
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = string.Format(""A test number: {0:N2}"", i);
	}
}");
        }

        [Test]
        public void TestComplexFormatString()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = …""A test number: "" + i.ToString(""N2"", culture);
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = string.Format(""A test number: {0}"", i.ToString(""N2"", culture));
	}
}");
        }

        [Test]
        public void TestFormatBracesRegular()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = …""A test number: {"" + i + ""}"";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = string.Format(""A test number: {{{0}}}"", i);
	}
}");
        }

        /*
        [Test]
        public void TestFormatBracesWithFormat()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = $""A test number: {"" + i.ToString(""N2"") + ""}"";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = string.Format (""A test number: {0}{1:N2}{2}"", '{', i, '}');
	}
}");
        }
         */

        [Test]
        public void TestUnnecessaryStringFormat()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		string res = …""String 1"" + ""String 2"";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		string res = ""String 1String 2"";
	}
}");
        }

        [Test]
        public void TestUnnecessaryToString()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
        int i = 42;
		string res = …""String 1"" + i.ToString();
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
        int i = 42;
		string res = string.Format(""String 1{0}"", i);
	}
}");
        }

        [Test]
        public void EscapeBraces()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = …""{"" + i + ""}"";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = string.Format(""{{{0}}}"", i);
	}
}");
        }
        
        /*
        This is far too complicated and likely to cause trouble.
        Imagine how an IDE might react to unique and Unicode chars like backspace, EOL, etc. inside a verbatim string.
        Escapded unicode chars in strings must be converted to literal chars, yuck.
        Looking at the original UseStringFormatAction, this complication does not seem to be accounted for and would have resulted in
        escaped sequences being rendered.

        [Test]
        public void QuotesMixedVerbatim()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = …""\"""" + i + @"""""""";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = string.Format (@""""""{0}"""""", i);
	}
}");
        }
        */
    }
}
