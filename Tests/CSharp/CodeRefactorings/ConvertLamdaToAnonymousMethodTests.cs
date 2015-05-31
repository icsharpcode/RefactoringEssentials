using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ConvertLamdaToAnonymousMethodTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void LambdaBlock()
        {
            Test<ConvertLambdaToAnonymousMethodCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
        System.Action<int, int> a = (i1, i2) $=> { System.Console.WriteLine (i1); };
    }
}", @"
class A
{
    void F ()
    {
        System.Action<int, int> a = delegate (int i1, int i2) { System.Console.WriteLine(i1); };
    }
}");
        }

        [Test]
        public void LambdaBlockWithComment()
        {
            Test<ConvertLambdaToAnonymousMethodCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
		// Some comment
        System.Action<int, int> a = (i1, i2) $=> { System.Console.WriteLine (i1); };
    }
}", @"
class A
{
    void F ()
    {
		// Some comment
        System.Action<int, int> a = delegate (int i1, int i2) { System.Console.WriteLine(i1); };
    }
}");
        }

        [Test]
        public void LambdaExpression()
        {
            Test<ConvertLambdaToAnonymousMethodCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
        System.Action<int, int> a = (i1, i2) $=> System.Console.WriteLine (i1);
    }
}", @"
class A
{
    void F ()
    {
        System.Action<int, int> a = delegate (int i1, int i2)
        {
            System.Console.WriteLine(i1);
        };
    }
}");
        }

        [Test]
        public void LambdaExpressionWithComment()
        {
            Test<ConvertLambdaToAnonymousMethodCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
		// Some comment
        System.Action<int, int> a = (i1, i2) $=> System.Console.WriteLine (i1);
    }
}", @"
class A
{
    void F ()
    {
		// Some comment
        System.Action<int, int> a = delegate (int i1, int i2)
        {
            System.Console.WriteLine(i1);
        };
    }
}");
        }

        [Test]
        public void NonVoidExpressionTest()
        {
            Test<ConvertLambdaToAnonymousMethodCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
        System.Func<int> f = () $=> 1;
    }
}", @"
class A
{
    void F ()
    {
        System.Func<int> f = delegate
        {
            return 1;
        };
    }
}");
        }

        [Test]
        public void ParameterLessLambdaTest()
        {
            Test<ConvertLambdaToAnonymousMethodCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
        System.Action a = () $=> { System.Console.WriteLine(); };
    }
}", @"
class A
{
    void F ()
    {
        System.Action a = delegate { System.Console.WriteLine(); };
    }
}");
        }
    }
}