using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ExtractWhileConditionToInternalIfStatementTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestBasicCase()
        {
            Test<ExtractWhileConditionToInternalIfStatementCodeRefactoringProvider>(@"
public class Main 
{
    public int Foo (int i)
    {
        $while (i < 10)
        {
            System.Console.WriteLine(i);
            i++;
        }

        return 2;
    }
}
", @"
public class Main 
{
    public int Foo (int i)
    {
        while (true)
        {
            if (i >= 10)
                break;
            System.Console.WriteLine(i);
            i++;
        }

        return 2;
    }
}
");
        }

        [Test]
        public void TestBasicCaseWithComment()
        {
            Test<ExtractWhileConditionToInternalIfStatementCodeRefactoringProvider>(@"
public class Main 
{
    public int Foo (int i)
    {
        // Some comment
        $while (i < 10)
        {
            System.Console.WriteLine(i);
            i++;
        }

        return 2;
    }
}
", @"
public class Main 
{
    public int Foo (int i)
    {
        // Some comment
        while (true)
        {
            if (i >= 10)
                break;
            System.Console.WriteLine(i);
            i++;
        }

        return 2;
    }
}
");
        }

        [Test]
        public void TestAddBlock()
        {
            Test<ExtractWhileConditionToInternalIfStatementCodeRefactoringProvider>(@"
public class Main 
{
    public int Foo (int i)
    {
        $while (i < 10)
            System.Console.WriteLine(i++);

        return 2;
    }
}
", @"
public class Main 
{
    public int Foo (int i)
    {
        while (true)
        {
            if (i >= 10)
                break;
            System.Console.WriteLine(i++);
        }

        return 2;
    }
}
");
        }

        [Test]
        public void TestRemoveEmptyStatement()
        {
            Test<ExtractWhileConditionToInternalIfStatementCodeRefactoringProvider>(@"
public class Main 
{
    public int Foo (int i)
    {
        $while (i++ < 10)
            ;

        return 2;
    }
}
", @"
public class Main 
{
    public int Foo (int i)
    {
        while (true)
        {
            if (i++ >= 10)
                break;
        }

        return 2;
    }
}
");
        }
    }
}