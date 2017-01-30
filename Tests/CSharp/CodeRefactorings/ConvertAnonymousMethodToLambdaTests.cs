using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertAnonymousMethodToLambdaTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void BasicTest()
        {
            Test<ConvertAnonymousMethodToLambdaCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
        System.Action<int, int> action = delegate$ (int i1, int i2) { System.Console.WriteLine(i1); };
    }
}", @"
class A
{
    void F ()
    {
        System.Action<int, int> action = (i1, i2) => System.Console.WriteLine(i1);
    }
}");
        }

        [Fact]
        public void VarDeclaration()
        {
            Test<ConvertAnonymousMethodToLambdaCodeRefactoringProvider>(@"
class A
{
    void F ()
    {
        var action = delegate$ (int i1, int i2) { System.Console.WriteLine(i1); System.Console.WriteLine(i2); };
    }
}", @"
class A
{
    void F ()
    {
        var action = (int i1, int i2) => { System.Console.WriteLine(i1); System.Console.WriteLine(i2); };
    }
}");
        }

        [Fact]
        public void ParameterLessDelegate()
        {
            Test<ConvertAnonymousMethodToLambdaCodeRefactoringProvider>(@"
class A
{
    void F (int i)
    {
        System.Action<int> act = $delegate { System.Console.WriteLine(i); };
    }
}", @"
class A
{
    void F (int i)
    {
        System.Action<int> act = obj => System.Console.WriteLine(i);
    }
}");
        }
    }
}

