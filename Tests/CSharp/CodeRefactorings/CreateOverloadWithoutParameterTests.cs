using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [Ignore("Needs insertion cursor mode.")]
    [TestFixture]
    public class CreateOverloadWithoutParameterTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void Test()
        {
            Test<CreateOverloadWithoutParameterAction>(@"
class Test
{
	void TestMethod (int $i)
	{
	}
}", @"
class Test
{
	void TestMethod ()
	{
		TestMethod (0);
	}
	void TestMethod (int i)
	{
	}
}");
        }

        [Test]
        public void TestByRefParameter()
        {
            Test<CreateOverloadWithoutParameterAction>(
                @"class Test
{
	void TestMethod (ref int $i)
	{
	}
}",
                @"class Test
{
	void TestMethod ()
	{
		int i = 0;
		TestMethod (ref i);
	}
	void TestMethod (ref int i)
	{
	}
}");
        }

        [Test]
        public void TestOutParameter()
        {
            Test<CreateOverloadWithoutParameterAction>(
                @"class Test
{
	void TestMethod (out int $i)
	{
	}
}",
                @"class Test
{
	void TestMethod ()
	{
		int i;
		TestMethod (out i);
	}
	void TestMethod (out int i)
	{
	}
}");
        }

        [Test]
        public void TestDefaultValue()
        {
            TestDefaultValue("object", "null");
            TestDefaultValue("dynamic", "null");
            TestDefaultValue("int?", "null");
            TestDefaultValue("bool", "false");
            TestDefaultValue("double", "0");
            TestDefaultValue("char", "'\\0'");
            TestDefaultValue("System.DateTime", "new System.DateTime ()");

            Test<CreateOverloadWithoutParameterAction>(@"
class Test
{
	void TestMethod<T> (T $i)
	{
	}
}", @"
class Test
{
	void TestMethod<T> ()
	{
		TestMethod (default(T));
	}
	void TestMethod<T> (T i)
	{
	}
}");
        }

        void TestDefaultValue(string type, string expectedValue)
        {
            Test<CreateOverloadWithoutParameterAction>(@"
class Test
{
	void TestMethod (" + type + @" $i)
	{
	}
}", @"
class Test
{
	void TestMethod ()
	{
		TestMethod (" + expectedValue + @");
	}
	void TestMethod (" + type + @" i)
	{
	}
}");
        }

        [Test]
        public void TestOptionalParameter()
        {
            TestWrongContext<CreateOverloadWithoutParameterAction>(
                @"class Test
{
	void TestMethod (int $i = 0)
	{
	}
}");
        }

        [Test]
        public void TestExistingMethod()
        {
            TestWrongContext<CreateOverloadWithoutParameterAction>(
                @"class Test
{
	void TestMethod (int c)
	{
	}
	void TestMethod (int a, int $b)
	{
	}
}");
            TestWrongContext<CreateOverloadWithoutParameterAction>(
                @"class Test
{
	void TestMethod <T> (T c)
	{
	}
	void TestMethod <T> (T a, T $b)
	{
	}
}");
        }

        [Test]
        public void TestExplicitImpl()
        {
            TestWrongContext<CreateOverloadWithoutParameterAction>(
                @"
interface ITest
{
	void Test (int a, int b);
}
class Test : ITest
{
	void ITest.Test (int a, int $b)
	{
	}
}");
        }

        [Test]
        public void TestGenereatedCall()
        {
            Test<CreateOverloadWithoutParameterAction>(
                @"class Test
{
	void TestMethod (ref int $i, ref int j, out int k)
	{
	}
}",
                @"class Test
{
	void TestMethod (ref int j, out int k)
	{
		int i = 0;
		TestMethod (ref i, ref j, out k);
	}
	void TestMethod (ref int i, ref int j, out int k)
	{
	}
}");
        }
    }
}
