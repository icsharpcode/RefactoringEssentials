using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertAutoPropertyToPropertyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleProperty()
        {
            Test<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(@"class TestClass
{
	string $Test { get; set; }
}", @"class TestClass
{
    string Test
    {
        get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}");
        }

        [Fact]
        public void TestExpressionBodyNoCrash()
        {
            TestWrongContext<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(@"class TestClass
{
	string $Test => ""Hello World!"";
}");
        }

        [Fact]
        public void TestInterfaceContext()
        {
            TestWrongContext<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(
                "interface Test { string $Test2 { get; set; } }");
            TestWrongContext<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(
                "interface Test { string $Test2 { get; } }");
        }

        [Fact]
        public void TestSimplify()
        {
            Test<ConvertAutoPropertyToPropertyCodeRefactoringProvider>(@"
using System;
class TestClass
{
	string $Test { get; set; }
}", @"
using System;
class TestClass
{
    string Test
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }
}");
        }
    }
}

