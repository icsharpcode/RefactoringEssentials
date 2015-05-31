using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ComputeConstantValueTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void Rational1()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		int a = 1 $+ 1;
	}
}", @"
class TestClass
{
	public void F()
	{
		int a = 2;
	}
}");
        }

        [Test]
        public void Rational1WithComment()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
        // Some comment
		int a = 1 $+ 1;
	}
}", @"
class TestClass
{
	public void F()
	{
        // Some comment
		int a = 2;
	}
}");
        }

        [Test]
        public void Rational2()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		int b = 2 $* 2;
	}
}", @"
class TestClass
{
	public void F()
	{
		int b = 4;
	}
}");
        }
        [Test]
        public void Rational3()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		double c = 0.2 $/ 2;
	}
}", @"
class TestClass
{
	public void F()
	{
		double c = 0.1;
	}
}");
        }
        [Test]
        public void Rational4()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		double d = 2 $* (-0.2);
	}
}", @"
class TestClass
{
	public void F()
	{
		double d = -0.4;
	}
}");
        }
        [Test]
        public void Rational5()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		int e = 2 $* (1 << 2);
	}
}", @"
class TestClass
{
	public void F()
	{
		int e = 8;
	}
}");
        }
        [Test]
        public void Rational6()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		int f = 1 $+ (~4);
	}
}", @"
class TestClass
{
	public void F()
	{
		int f = -4;
	}
}");
        }

        [Test]
        public void Bool1()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		bool a = $!(true);
	}
}", @"
class TestClass
{
	public void F()
	{
		bool a = false;
	}
}");
        }

        [Test]
        public void Bool2()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		bool b = $!!(!!!(true & false));
	}
}", @"
class TestClass
{
	public void F()
	{
		bool b = true;
	}
}");
        }

        [Test]
        public void Bool3()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		bool c = 1 $> 0;
	}
}", @"
class TestClass
{
	public void F()
	{
		bool c = true;
	}
}");
        }

        [Test]
        public void String1()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		string a = ""a""$+""b"";
	}
}", @"
class TestClass
{
	public void F()
	{
		string a = ""ab"";
	}
}");
        }

        [Test]
        public void UseConstant()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	const double pi = 3.141;
	public void F()
	{
		double pi2 = $2 * pi;
	}
}", @"
class TestClass
{
	const double pi = 3.141;
	public void F()
	{
		double pi2 = 6.282;
	}
}");
        }


        [Test]
        public void Invalid()
        {
            TestWrongContext<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		bool a = !(true);
		bool b = $!!(!!!(true & a));
	}
}");
        }


        [Test]
        public void TestWrongHotSpot()
        {
            TestWrongContext<ComputeConstantValueCodeRefactoringProvider>(@"
class TestClass
{
	public void F()
	{
		int a = 1 +$ 1;
	}
}");
        }
    }
}
