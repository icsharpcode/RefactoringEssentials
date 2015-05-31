using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture, Ignore("Not implemented!")]
    public class IntroduceConstantTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestLocalConstant()
        {
            Test<IntroduceConstantAction>(@"class TestClass
{
	public void Hello ()
	{
		System.Console.WriteLine ($""Hello World"");
	}
}", @"class TestClass
{
	public void Hello ()
	{
		const string helloWorld = ""Hello World"";
		System.Console.WriteLine (helloWorld);
	}
}");
        }

        [Test]
        public void TestLocalConstantHexNumber()
        {
            Test<IntroduceConstantAction>(@"class TestClass
{
	public void Hello ()
	{
		System.Console.WriteLine ($0xAFFE);
	}
}", @"class TestClass
{
	public void Hello ()
	{
		const int i = 0xAFFE;
		System.Console.WriteLine (i);
	}
}");
        }

        [Test]
        public void TestFieldConstant()
        {
            Test<IntroduceConstantAction>(@"class TestClass
{
	public void Hello ()
	{
		System.Console.WriteLine ($""Hello World"");
	}
}", @"class TestClass
{
	const string helloWorld = ""Hello World"";
	public void Hello ()
	{
		System.Console.WriteLine (helloWorld);
	}
}", 1);
        }

        [Test]
        public void TestLocalConstantReplaceAll()
        {
            Test<IntroduceConstantAction>(@"class TestClass
{
	public void Hello ()
	{
		System.Console.WriteLine ($""Hello World"");
		System.Console.WriteLine (""Hello World"");
		System.Console.WriteLine (""Hello World"");
	}
}", @"class TestClass
{
	public void Hello ()
	{
		const string helloWorld = ""Hello World"";
		System.Console.WriteLine (helloWorld);
		System.Console.WriteLine (helloWorld);
		System.Console.WriteLine (helloWorld);
	}
}", 2);
        }

        [Test]
        public void TestAlreadyConstant()
        {
            TestWrongContext<IntroduceConstantAction>(@"class TestClass
{
	public void Hello ()
	{
		const int i = $0xAFFE;
	}
}");
        }

        [Test]
        public void TestAlreadyConstantCase2()
        {
            TestWrongContext<IntroduceConstantAction>(@"class TestClass
{
	const int i = $0xAFFE;
}");
        }

        [Test]
        public void TestIntroduceConstantInInitializer()
        {
            Test<IntroduceConstantAction>(@"class TestClass
{
	readonly int foo = new Foo ($5);
}", @"class TestClass
{
	const int i = 5;
	readonly int foo = new Foo (i);
}");
        }
    }
}