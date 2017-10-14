using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ConvertDoLoopWhileToDoWhileLoopTests : VBCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

