using System;
using Xunit;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ContractRequiresStringNotNullOrEmptyTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestLambda()
        {
            Test<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = (string $s, int e) => {
        };
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test ()
    {
        var lambda = (string s, int e) => {
            Contract.Requires(string.IsNullOrEmpty(s) == false);
        };
    }
}");
        }

        [Fact]
        public void TestAnonymousMethod()
        {
            Test<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = delegate(string $-[s]-, object e) {
        };
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test ()
    {
        var lambda = delegate(string s, object e) {
            Contract.Requires(string.IsNullOrEmpty(s) == false);
        };
    }
}");
        }

        [Fact]
        public void TestContractAlreadyPresentEqualsFalseFormat()
        {
            TestWrongContext<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = (string $s, int e) => {
            Contract.Requires(string.IsNullOrEmpty(s) == false);
        };
    }
}");
        }

        [Fact]
        public void TestContractAlreadyPresentFalseEqualsFormat()
        {
            TestWrongContext<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = (string $s, int e) => {
            Contract.Requires(false==string.IsNullOrEmpty(s));
        };
    }
}");
        }

        [Fact]
        public void TestContractAlreadyPresentNegateFormat()
        {
            TestWrongContext<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = (string $s, int e) => {
            Contract.Requires(!string.IsNullOrEmpty(s));
        };
    }
}");
        }

        [Fact]
        public void TestDifferentContractAlreadyPresent()
        {
            Test<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test ()
    {
        var lambda = (string $s, int e) => {
            Contract.Requires(string.IsNullOrEmpty(notS) == false);
        };
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test ()
    {
        var lambda = (string s, int e) => {
            Contract.Requires(string.IsNullOrEmpty(s) == false);
            Contract.Requires(string.IsNullOrEmpty(notS) == false);
        };
    }
}");
        }

        [Fact]
        public void TestUsingStatementAlreadyPresent()
        {
            Test<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"using System.Diagnostics.Contracts;
class Foo
{
    void Test ()
    {
        var lambda = (string $s, int e) => {
        };
    }
}", @"using System.Diagnostics.Contracts;
class Foo
{
    void Test ()
    {
        var lambda = (string s, int e) => {
            Contract.Requires(string.IsNullOrEmpty(s) == false);
        };
    }
}");
        }

        [Fact]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
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

            Test<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
{
    void Test (string $test)
    {
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    void Test (string test)
    {
        Contract.Requires(string.IsNullOrEmpty(test) == false);
    }
}", parseOptions: parseOptions);
        }
    }
}
