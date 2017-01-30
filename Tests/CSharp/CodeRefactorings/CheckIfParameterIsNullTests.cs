using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class CheckIfParameterIsNullTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            string result = RunContextAction(
                                         new CheckIfParameterIsNullCodeRefactoringProvider(),
                                         "using System;" + Environment.NewLine +
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test (string $param)" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        Console.WriteLine (param);" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test (string param)" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        if (param == null)" + Environment.NewLine +
                "            throw new ArgumentNullException(nameof(param));" + Environment.NewLine +
                "        Console.WriteLine (param);" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact(Skip="Broken.")]
        public void TestWithComment()
        {
            string result = RunContextAction(
                                         new CheckIfParameterIsNullCodeRefactoringProvider(),
                                         "using System;" + Environment.NewLine +
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test (string $param)" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        // Some comment" + Environment.NewLine +
                                         "        Console.WriteLine (param);" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test (string param)" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        if (param == null)" + Environment.NewLine +
                "            throw new ArgumentNullException(\"param\");" + Environment.NewLine +
                "        // Some comment" + Environment.NewLine +
                "        Console.WriteLine (param);" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestLambda()
        {
            Test<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
        };
    }
}", @"class Foo
{
    void Test ()
    {
        var lambda = (sender, e) => {
            if (sender == null)
                throw new System.ArgumentNullException(nameof(sender));
        };
    }
}");
        }

        [Fact]
        public void TestAnonymousMethod()
        {
            Test<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = delegate(object $-[sender]-, object e) {
        };
    }
}", @"class Foo
{
    void Test ()
    {
        var lambda = delegate(object sender, object e) {
            if (sender == null)
                throw new System.ArgumentNullException(nameof(sender));
        };
    }
}");
        }

        [Fact]
        public void TestNullCheckAlreadyThere_StringName()
        {
            TestWrongContext<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
            if (sender == null)
                throw new System.ArgumentNullException(""sender"");
        };
    }
}");
        }

        [Fact]
        public void TestNullCheckAlreadyThere_NameOf()
        {
            TestWrongContext<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
            if (sender == null)
                throw new System.ArgumentNullException(nameof(sender));
        };
    }
}");
        }

        [Fact]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
	void Test ($string param)
	{
	}
}");
        }


        [Fact]
        public void Test_OldCSharp()
        {
            var parseOptions = new CSharpParseOptions(
                LanguageVersion.CSharp5,
                DocumentationMode.Diagnose | DocumentationMode.Parse,
                SourceCodeKind.Regular,
                ImmutableArray.Create("DEBUG", "TEST")
            );

            Test<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
    void Test (string $test)
    {
    }
}", @"class Foo
{
    void Test (string test)
    {
        if (test == null)
            throw new System.ArgumentNullException(""test"");
    }
}", parseOptions: parseOptions);
        }
    }
}
