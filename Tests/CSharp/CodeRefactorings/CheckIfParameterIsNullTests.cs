using System;
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CheckIfParameterIsNullTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

            Assert.AreEqual(
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

        [Ignore("broken")]
        [Test]
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

            Assert.AreEqual(
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<CheckIfParameterIsNullCodeRefactoringProvider>(@"class Foo
{
	void Test ($string param)
	{
	}
}");
        }


        [Test]
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
