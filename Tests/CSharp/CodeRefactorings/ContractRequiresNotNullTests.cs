using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ContractRequiresNotNullTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

            Assert.Equal(
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<ContractRequiresNotNullCodeRefactoringProvider>(@"class Foo
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
