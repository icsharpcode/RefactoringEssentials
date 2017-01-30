using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ContractEnsuresNotNullReturnTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
        public void VoidReturnType()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
            {
                public $void Foo() 
                { 
                }
            }");
        }

        [Fact]
        // Not sure why anyone would want this when can just change the return type to be non nullable. Maybe you have to implement an interface you don't control or something strange like that.
        public void NullableValueTypeReturnType()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    $public int? Foo() 
    { 
    }
}", @"using System.Diagnostics.Contracts;

class Test
{
    public int? Foo() 
    {
        Contract.Ensures(Contract.Result<int?>() != null);
    }
}");
        }

        [Fact]
        public void ObjectReturnType()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    $public Cedd Foo() 
    { 
    }
}", @"using System.Diagnostics.Contracts;

class Test
{
    public Cedd Foo() 
    {
        Contract.Ensures(Contract.Result<Cedd>() != null);
    }
}");
        }

        [Fact]
        public void UsingStatementAlreadyThere()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"using System.Diagnostics.Contracts;

class Test
{
    $public Cedd Foo() 
    { 
    }
}", @"using System.Diagnostics.Contracts;

class Test
{
    public Cedd Foo() 
    {
        Contract.Ensures(Contract.Result<Cedd>() != null);
    }
}");
        }

        [Fact]
        public void TestContractEnsuresReturnAlreadyThere()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(@"class Test
{
    public $Cedd Foo() 
    {
        Contract.Ensures(Contract.Result<Cedd>() != null);
    }
}");
        }

        [Fact]
        public void TestContractEnsuresReturnAlreadyThereWithWhitespace()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(@"class Test
{
    public $Cedd Foo() 
    {
        Contract.Ensures(Contract.Result<Cedd>() !=     null);
    }
}");
        }

        [Fact]
        public void TestContractEnsuresReturnAlreadyThereReversedParameters()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(@"class Test
{
    public $Cedd Foo() 
    {
        Contract.Ensures(null != Contract.Result<Cedd>());
    }
}");
        }

        [Fact]
        public void ObjectPropertyGetter()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    public Cedd Foo
    {
        $get
        {
            return null;
        }
    }
}", @"using System.Diagnostics.Contracts;

class Test
{
    public Cedd Foo
    {
        get
        {
            Contract.Ensures(Contract.Result<Cedd>() != null);
            return null;
        }
    }
}");
        }

        [Fact]
        public void ContractEnsuresReturnAlreadyThereForPropertyGetter()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    public Cedd Foo
    {
        $get
        {
            Contract.Ensures(Contract.Result<Cedd>() != null);
            return null;
        }
    }
}");
        }

        [Fact]
        public void UsingStatementAlreadyThereForPropertyGetter()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"using System.Diagnostics.Contracts;
class Test
{
    public Cedd Foo
    {
        $get
        {
            return null;
        }
    }
}", @"using System.Diagnostics.Contracts;
class Test
{
    public Cedd Foo
    {
        get
        {
            Contract.Ensures(Contract.Result<Cedd>() != null);
            return null;
        }
    }
}");
        }

        [Fact]
        public void ObjectPropertyIndexer()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    public Cedd this[int index]
    {
        $get
        {
            return null;
        }
    }
}", @"using System.Diagnostics.Contracts;

class Test
{
    public Cedd this[int index]
    {
        get
        {
            Contract.Ensures(Contract.Result<Cedd>() != null);
            return null;
        }
    }
}");
        }

        [Fact]
        public void NullablePropertyIndexer()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    public double? this[int index]
    {
        $get
        {
            return null;
        }
    }
}", @"using System.Diagnostics.Contracts;

class Test
{
    public double? this[int index]
    {
        get
        {
            Contract.Ensures(Contract.Result<double?>() != null);
            return null;
        }
    }
}");
        }

        [Fact]
        public void ContractEnsuresReturnAlreadyThereForPropertyIndexer()
        {
            TestWrongContext<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"class Test
{
    public Cedd this[int index]
    {
        $get
        {
            Contract.Ensures(Contract.Result<Cedd>() != null);
            return null;
        }
    }
}");
        }

        [Fact]
        public void UsingStatementAlreadyThereForPropertyIndexer()
        {
            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(
                @"using System.Diagnostics.Contracts;
class Test
{
    public Cedd this[int index]
    {
        $get
        {
            return null;
        }
    }
}", @"using System.Diagnostics.Contracts;
class Test
{
    public Cedd this[int index]
    {
        get
        {
            Contract.Ensures(Contract.Result<Cedd>() != null);
            return null;
        }
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

            Test<ContractEnsuresNotNullReturnCodeRefactoringProvider>(@"class Foo
{
    Cedd $Test ()
    {
    }
}", @"using System.Diagnostics.Contracts;

class Foo
{
    Cedd Test ()
    {
        Contract.Ensures(Contract.Result<Cedd>() != null);
    }
}", parseOptions: parseOptions);
        }

    }
}
