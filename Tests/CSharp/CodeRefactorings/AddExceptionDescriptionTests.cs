using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class AddExceptionDescriptionTests : CSharpCodeRefactoringTestBase
    {

        [Test]
        public void TestPlainEntity()
        {
            TestWrongContext<AddExceptionDescriptionCodeRefactoringProvider>(@"using System;
public class Test
{
    public void Bar(Test test)
    {
        if (test == null)
            $throw new ArgumentNullException(""test"");
    }
}
");
        }

        [Test]
        public void TestAddToExistingDocumentation()
        {
            Test<AddExceptionDescriptionCodeRefactoringProvider>(@"using System;
public class Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""test""></param>
    public void Bar(Test test)
    {
        if (test == null)
            $throw new ArgumentNullException(""test"");
    }
}
", @"using System;
public class Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""test""></param>
    /// <exception cref=""T:System.ArgumentNullException""></exception>
    public void Bar(Test test)
    {
        if (test == null)
            throw new ArgumentNullException(""test"");
    }
}
");
        }

        [Test]
        public void TestAddRethrowToExistingDocumentation1()
        {
            Test<AddExceptionDescriptionCodeRefactoringProvider>(@"using System;
public class Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""test""></param>
    public void Bar(Test test)
    {
        try
        {
            // Something throwing exception
        }
        catch (ArgumentNullException)
        {
            $throw;
        }
    }
}
", @"using System;
public class Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""test""></param>
    /// <exception cref=""T:System.ArgumentNullException""></exception>
    public void Bar(Test test)
    {
        try
        {
            // Something throwing exception
        }
        catch (ArgumentNullException)
        {
            throw;
        }
    }
}
");
        }

        [Test]
        public void TestAddRethrowToExistingDocumentation2()
        {
            Test<AddExceptionDescriptionCodeRefactoringProvider>(@"using System;
public class Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""test""></param>
    public void Bar(Test test)
    {
        try
        {
            // Something throwing exception
        }
        catch (ArgumentNullException ex)
        {
            $throw ex;
        }
    }
}
", @"using System;
public class Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""test""></param>
    /// <exception cref=""T:System.ArgumentNullException""></exception>
    public void Bar(Test test)
    {
        try
        {
            // Something throwing exception
        }
        catch (ArgumentNullException ex)
        {
            throw ex;
        }
    }
}
");
        }

        [Test]
        public void TestAlreadyAdded()
        {
            TestWrongContext<AddExceptionDescriptionCodeRefactoringProvider>(@"
using System;
public class Test
{
    /// <exception cref=""T:System.ArgumentNullException""></exception>
    public void Bar(Test test)
    {
        if (test == null) 
            $throw new ArgumentNullException(""test"");
    }
}
");
        }
    }
}

