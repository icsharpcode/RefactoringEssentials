using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture, Ignore("Not implemented!")]
    public class UseStringFormatTests : CSharpCodeRefactoringTestBase
    {
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
		string str = string.Format (""{0}test{1}test{2}"", 1 + 2, 1, 1.1);
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
		string str = string.Format (@""
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
		string str = $""test"" + i + ""test"" + i;
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = string.Format (""test{0}test{0}"", i);
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
		string res = $""A test number: "" + i.ToString(""N2"");
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = string.Format (""A test number: {0:N2}"", i);
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
		string res = $""A test number: {"" + i + ""}"";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 42;
		string res = string.Format (""A test number: {{{0}}}"", i);
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
		string res = $""String 1"" + ""String 2"";
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
		string res = $""String 1"" + i.ToString();
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
        int i = 42;
		string res = string.Format (""String 1{0}"", i);
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
		string str = $""{"" + i + ""}"";
	}
}", @"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = string.Format (""{{{0}}}"", i);
	}
}");
        }

        [Test]
        public void QuotesMixedVerbatim()
        {
            Test<UseStringFormatAction>(@"
class TestClass
{
	void TestMethod ()
	{
		int i = 0;
		string str = $""\"""" + i + @"""""""";
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
    }
}
