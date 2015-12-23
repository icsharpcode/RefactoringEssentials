using System;
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ContractRequiresNotNullTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void Test()
        {
            string result = RunContextAction(
                                         new ContractRequiresNotNullCodeRefactoringProvider(),
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
                "using System.Diagnostics.Contracts;" + Environment.NewLine + Environment.NewLine +
            "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test (string param)" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        Contract.Requires(param != null);" + Environment.NewLine +
                "        Console.WriteLine (param);" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Test]
        public void TestLambda()
        {
            Test<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
        };
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test ()
    {
        var lambda = (sender, e) => {
            Contract.Requires(sender != null);
        };
    }
}");
        }

        [Test]
        public void TestAnonymousMethod()
        {
            Test<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = delegate(object $-[sender]-, object e) {
        };
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test ()
    {
        var lambda = delegate(object sender, object e) {
            Contract.Requires(sender != null);
        };
    }
}");
        }

        [Test]
        public void TestContractRequiresNotNullCheckAlreadyThere()
        {
            TestWrongContext<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
            Contract.Requires(sender != null);
        };
    }
}");
        }

        [Test]
        public void TestContractRequiresNotNullCheckNotAlreadyThere()
        {
            Test<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
            Contract.Requires(notSender != null);
        };
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test ()
    {
        var lambda = (sender, e) => {
            Contract.Requires(sender != null);
            Contract.Requires(notSender != null);
        };
    }
}");
        }

        [Test]
        public void TestUsingStatementAlreadyThere()
        {
            Test<ContractRequiresNotNullCodeRefactoringProvider>(@"using System.Diagnostics.Contracts;
class Foo
{
    void Test ()
    {
        var lambda = ($sender, e) => {
        };
    }
}", @"using System.Diagnostics.Contracts;
class Foo
{
    void Test ()
    {
        var lambda = (sender, e) => {
            Contract.Requires(sender != null);
        };
    }
}");
        }

        [Test]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
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

            Test<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
{
    void Test (string $test)
    {
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test (string test)
    {
        Contract.Requires(test != null);
    }
}", parseOptions: parseOptions);
        }
    }
}
