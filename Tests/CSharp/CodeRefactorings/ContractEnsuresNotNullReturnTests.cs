using System;
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;
using System.Diagnostics.Contracts;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ContractEnsuresNotNullReturnTests : CSharpCodeRefactoringTestBase
    {
        public int? play()
        {
            Contract.Ensures(Contract.Result<int?>() != null);
            return null;
        }

        public ContractEnsuresNotNullReturnCodeRefactoringProvider play2()
        {
            return null;
        }

        [Test]
        public void ValueTypeReturnType()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
            {
                public $int Foo() 
                { 
                }
            }");
        }

        [Test]
        public void NullableValueTypeLocalVariable()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
            {
                public void Foo() 
                { 
                    $int? foo;
                }
            }");
        }

        [Test]
        // Not sure why anyone would want this when can just change the return type to be non nullable. Maybe you have to implement an interface you don't control or something strange like that.
        public void NullableValueTypeReturnType()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    $public int? Foo() 
    { 
    }
}", @"class Test
{
    public int? Foo() 
    {
        Contract.Ensures(Contract.Result<int?>() != null);
    }
}");
        }

        [Test]
        public void ObjectReturnType()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    $public Cedd Foo() 
    { 
    }
}", @"class Test
{
    public Cedd Foo() 
    {
        Contract.Ensures(Contract.Result<Cedd>() != null);
    }
}");
        }

        // test add using statement
        // test statement already exists
        // TestLambda?
        // TestAnonymousMethod?
        // test a different contract.requires is already there
        // test using statement already there
        // test old csharp?
        // anything else? 

    }
}
