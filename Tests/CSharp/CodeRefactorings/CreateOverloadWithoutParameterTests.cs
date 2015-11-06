using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CreateOverloadWithoutParameterTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void Test()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
class Test
{
    void TestMethod (int $i)
    {
    }
}", @"
class Test
{
    void TestMethod()
    {
        TestMethod(0);
    }
    void TestMethod (int i)
    {
    }
}");
        }

        [Test]
        public void TestWithReturnValue()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
class Test
{
    int TestMethod (int $i)
    {
        return -1;
    }
}", @"
class Test
{
    int TestMethod()
    {
        return TestMethod(0);
    }
    int TestMethod (int i)
    {
        return -1;
    }
}");
        }

        [Test]
        public void TestWithXmlDoc()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
class Test
{
    /// <summary>
    /// Some description
    /// </summary>
    void TestMethod (int $i)
    {
    }
}", @"
class Test
{
    void TestMethod()
    {
        TestMethod(0);
    }
    /// <summary>
    /// Some description
    /// </summary>
    void TestMethod (int i)
    {
    }
}");
        }

        [Test]
        public void TestByRefParameter()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(
                @"class Test
{
    void TestMethod (ref int $i)
    {
    }
}",
                @"class Test
{
    void TestMethod()
    {
        int i = 0;
        TestMethod(ref i);
    }
    void TestMethod (ref int i)
    {
    }
}");
        }

        [Test]
        public void TestOutParameter()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(
                @"class Test
{
    void TestMethod (out int $i)
    {
    }
}",
        @"class Test
{
    void TestMethod()
    {
        int i;
        TestMethod(out i);
    }
    void TestMethod (out int i)
    {
    }
}");
        }

        [Test]
        public void TestDefaultValue()
        {
            TestDefaultValue("object", null, "null");
            TestDefaultValue("dynamic", null, "null");
            TestDefaultValue("int?", null, "null");
            TestDefaultValue("System.Nullable<T>", null, "null");
            TestDefaultValue("System.Collections.Generic.IEnumerable<int>", null, "null");
            TestDefaultValue("System.Collections.Generic.IEnumerable", "T", "default(System.Collections.Generic.IEnumerable<T>)");
            TestDefaultValue("bool", null, "false");
            TestDefaultValue("double", null, "0");
            TestDefaultValue("char", null, "'\\0'");
            TestDefaultValue("System.DateTime", null, "new System.DateTime()");

            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
class Test
{
    void TestMethod<T> (T $i)
    {
    }
}", @"
class Test
{
    void TestMethod<T>()
    {
        TestMethod(default(T));
    }
    void TestMethod<T> (T i)
    {
    }
}");
        }

        void TestDefaultValue(string type, string typeParameter, string expectedValue)
        {
            string generic = string.IsNullOrEmpty(typeParameter) ? "" : ("<" + typeParameter + ">");

            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
class Test
{
    void TestMethod" + generic + " (" + type + generic + @" $i)
    {
    }
}", @"
class Test
{
    void TestMethod" + generic + @"()
    {
        TestMethod(" + expectedValue + @");
    }
    void TestMethod" + generic + " (" + type + generic + @" i)
    {
    }
}");
        }

        [Test]
        public void TestOptionalParameter()
        {
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(
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
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(
                @"class Test
{
    void TestMethod (int c)
    {
    }
    void TestMethod (int a, int $b)
    {
    }
}");
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(
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
        public void TestInterface()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
interface ITest
{
    void Test (int $a, int b);
}", @"
interface ITest
{
    void Test(int b);
    void Test (int a, int b);
}");
        }

        [Test]
        public void TestExplicitImpl()
        {
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(
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
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(
                @"class Test
{
    void TestMethod (ref int $i, ref int j, out int k)
    {
    }
}",
                @"class Test
{
    void TestMethod(ref int j, out int k)
    {
        int i = 0;
        TestMethod(ref i, ref j, out k);
    }
    void TestMethod (ref int i, ref int j, out int k)
    {
    }
}");
        }
    }
}
