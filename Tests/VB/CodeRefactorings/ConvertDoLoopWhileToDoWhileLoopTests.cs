using NUnit.Framework;
using RefactoringEssentials.VB.CodeRefactorings;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    [TestFixture]
    public class ConvertDoLoopWhileToDoWhileLoopTests : VBCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleWhile()
        {
            Test<ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider>(@"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do
            x += 1
        $Loop While x <> 1
    End Sub
End Class", @"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do While x <> 1
            x += 1
        Loop
    End Sub
End Class");
        }

        [Test]
        public void TestWhileFromSimpleDoStatement()
        {
            Test<ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider>(@"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        $Do
            x += 1
        Loop While x <> 1
    End Sub
End Class", @"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do While x <> 1
            x += 1
        Loop
    End Sub
End Class");
        }

        [Test]
        public void TestSimpleUntil()
        {
            Test<ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider>(@"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do
            x += 1
        $Loop Until x <> 1
    End Sub
End Class", @"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do Until x <> 1
            x += 1
        Loop
    End Sub
End Class");
        }

        [Test]
        public void TestSimpleWithComment1()
        {
            Test<ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider>(@"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        ' Some comment
        Do
            x += 1
        $Loop While x <> 1
    End Sub
End Class", @"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        ' Some comment
        Do While x <> 1
            x += 1
        Loop
    End Sub
End Class");
        }

        [Test]
        public void TestSimpleWithComment2()
        {
            Test<ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider>(@"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do
            x += 1 ' Some comment
        $Loop While x <> 1
    End Sub
End Class", @"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do While x <> 1
            x += 1 ' Some comment
        Loop
    End Sub
End Class");
        }

        [Test]
        public void TestDisabledInContent()
        {
            TestWrongContext<ConvertDoLoopWhileToDoWhileLoopCodeRefactoringProvider>(@"
Class Foo
    Sub TestMethod()
        Dim x As Integer = 1
        Do
            $x += 1
        Loop While x <> 1
    End Sub
End Class");
        }
    }
}

