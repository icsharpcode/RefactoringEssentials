using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class UseStringFormatTests : CSharpCodeRefactoringTestBase
    {
		[Theory]
        [InlineData(@"if (…str != null && str != """") {}")]
        [InlineData(@"if (str …!= null && str != """") {}")]
        [InlineData(@"if (str != …null && str != """") {}")]
        [InlineData(@"if (str != null …&& str != """") {}")]
        [InlineData(@"if (str != null && …str != """") {}")]
        [InlineData(@"if (str != null && str …!= """") {}")]
        [InlineData(@"if (str != null && str != …"""") {}")]
        // Do not suggest action for binary expressions with assignment operators. https://github.com/icsharpcode/RefactoringEssentials/issues/137"
        public void Issue137_Case1(string expression)
        {
            TestWrongContext<UseStringFormatAction>(
                @"
class TestClass
{
    void TestMethod(string str)
    {
        " + expression + @"
    }
}
");
        }

        [Fact]
        // Do not suggest action for binary expressions with assignment operators. https://github.com/icsharpcode/RefactoringEssentials/issues/137
        public void Issue137_Case2()
        {
            TestWrongContext<UseStringFormatAction>(
                @"
class TestClass
{
    void TestMethod(string str)
    {
        var x = from w in new int[0]
                    let two = w * 2
                    let three = w * 3
                    let four = w * 4
                    select w + two + …three + four into sum
                    select sum * 2;
    }
}
");

        }

		[Theory]
        [InlineData(@"Assert.IsTrue(actions == null || actions.Count == 0, …action.GetType() + "" shouldn't be valid there."");")]
        [InlineData(@"Assert.IsTrue(actions == null || actions.Count == 0, action.GetType() …+ "" shouldn't be valid there."");")]
        [InlineData(@"Assert.IsTrue(actions == null || actions.Count == 0, action.GetType() + …"" shouldn't be valid there."");")]
        // Ensure proper handling across complication cases. https://github.com/icsharpcode/RefactoringEssentials/issues/137
        public void Issue137_Case3(string expression)
        {
            Test<UseStringFormatAction>(
                @"
class TestClass
{
    void TestMethod(string str, bool option1, bool option2)
    {
        " + expression + @"
    }
}", @"
class TestClass
{
    void TestMethod(string str, bool option1, bool option2)
    {
        Assert.IsTrue(actions == null || actions.Count == 0, string.Format(""{0} shouldn't be valid there."", action.GetType()));
    }
}");
        }

		[Theory]
        [InlineData(@"var x = …(option1 && option2 ? ""Hello "" : string.Empty) + ""World"";")]
        [InlineData(@"var x = (…option1 && option2 ? ""Hello "" : string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 …&& option2 ? ""Hello "" : string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 && …option2 ? ""Hello "" : string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 && option2 …? ""Hello "" : string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 && option2 ? …""Hello "" : string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 && option2 ? ""Hello "" …: string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 && option2 ? ""Hello "" : …string.Empty) + ""World"";")]
        [InlineData(@"var x = (option1 && option2 ? ""Hello "" : string.Empty…) + ""World"";")]
        [InlineData(@"var x = (option1 && option2 ? ""Hello "" : string.Empty) …+ ""World"";")]
        [InlineData(@"var x = (option1 && option2 ? ""Hello "" : string.Empty) + …""World"";")]
        // Ensure proper handling across complication cases. https://github.com/icsharpcode/RefactoringEssentials/issues/137
        public void Issue137_Case4(string expression)
        {
            Test<UseStringFormatAction>(
                @"
class TestClass
{
    void TestMethod(string str, bool option1, bool option2)
    {
        " + expression + @"
    }
}", @"
class TestClass
{
    void TestMethod(string str, bool option1, bool option2)
    {
        var x = string.Format(""{0}World"", (option1 && option2 ? ""Hello "" : string.Empty));
    }
}");
        }

		[Theory]
        [InlineData(@"var s = …""Hello"" + ""World!"";", @"var s = ""HelloWorld!"";")]
        [InlineData(@"var s = ""Hello"" …+ ""World!"";", @"var s = ""HelloWorld!"";")]
        [InlineData(@"var s = ""Hello"" + …""World!"";", @"var s = ""HelloWorld!"";")]
        // String concatenation alone should not be replaced by concatenated string.
        public void TestSimpleStringConcatenation(string expression, string expectation)
        {
            Test<UseStringFormatAction>(
                expression,
                expectation);
        }

        [InlineData(@"var s = …@""Hello"" + @""World!"";", @"var s = @""HelloWorld!"";")]
        [InlineData(@"var s = @""Hello"" …+ @""World!"";", @"var s = @""HelloWorld!"";")]
        [InlineData(@"var s = @""Hello"" + …@""World!"";", @"var s = @""HelloWorld!"";")]
        // String concatenation alone should not be replaced by concatenated string.
        public void TestSimpleVerbatimStringConcatenation(string expression, string expectation)
        {
            Test<UseStringFormatAction>(
                expression,
                expectation);
        }

        [Fact]
        // Concatenation between verbatim and non-verbatim strings is messy, so leave it alone.
        public void TestWrongContextForVerbatimAndNonVerbatimStrings()
        {
            TestWrongContext<UseStringFormatAction>(@"var s = @""Hello"" + …""World!"";");
        }

        [Fact]
        // Ensure action is not applied to addition syntax without string literals.
        public void TestWrongContextForAddition()
        {
            TestWrongContext<UseStringFormatAction>(@"var s = 1 + …2;");
        }

		[Theory]
        [InlineData(@"string str = …""Hello \x143 "" + a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""…Hello \x143 "" + a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello …\x143 "" + a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 …"" + a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" …+ a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + …a.Foo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.F…oo(""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(…""asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""…asdf"") + "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""asdf"") …+ "" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""asdf"") + …"" world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""asdf"") + "" …world! "" + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""asdf"") + "" world! ""… + a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""asdf"") + "" world! "" + …a.Bar(""jkl;"");")]
        [InlineData(@"string str = ""Hello \x143 "" + a.Foo(""asdf"") + "" world! "" + a.Bar(…""jkl;"");")]
        // Concatenation with member access to identifiers should be modified to use string.Format()
        public void TestIdentifiersWithMemberAccess(string expression)
        {
            Test<UseStringFormatAction>(
                @"class TestClass
                {
                    void TestMethod()
                    {
                        " + expression + @"
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


        [Fact]
        // Concatenation between verbatim strings and expressions is replaced with string.Format()
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

		[Theory]
        [InlineData(@"string str = …1 + 2 + ""test"" + 1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 …+ 2 + ""test"" + 1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + …2 + ""test"" + 1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 …+ ""test"" + 1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 + …""test"" + 1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 + ""test"" …+ 1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 + ""test"" + …1 + ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 + ""test"" + 1 …+ ""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 + ""test"" + 1 + …""test"" + 1.1;")]
        [InlineData(@"string str = 1 + 2 + ""test"" + 1 + ""test"" …+ 1.1;")]
        [InlineData(@"string str = 1 + 2 + ""test"" + 1 + ""test"" + …1.1;")]
        public void Test(string expression)
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		" + expression + @"
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
