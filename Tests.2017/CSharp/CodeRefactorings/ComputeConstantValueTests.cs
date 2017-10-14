using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ComputeConstantValueTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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
        [Fact]
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
        [Fact]
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
        [Fact]
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
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
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


        [Fact]
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
