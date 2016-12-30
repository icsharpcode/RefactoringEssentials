using System;
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ContractRequiresStringNotNullOrEmptyTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<ContractRequiresStringNotNullOrEmptyCodeRefactoringProvider>(@"class Foo
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
