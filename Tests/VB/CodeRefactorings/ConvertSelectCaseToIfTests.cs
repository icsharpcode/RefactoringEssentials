using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ConvertSelectCaseToIfTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void TestReturn()
        {
            Test<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        $Select Case a
            Case 0
                Return 0
            Case 1, 2
                Return 1
            Case 3, 4, 5
                Return 1
            Case Else
                Return 2
        End Select
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        If a = 0 Then
            Return 0
        ElseIf a = 1 OrElse a = 2 Then
            Return 1
        ElseIf a = 3 OrElse a = 4 OrElse a = 5 Then
            Return 1
        Else
            Return 2
        End If
    End Function
End Class");
        }

        [Fact]
        public void TestWithoutDefault()
        {
            Test<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        $Select Case a
            Case 0
                Return 0
            Case 1, 2
                Return 1
            Case 3, 4, 5
                Return 1
        End Select
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        If a = 0 Then
            Return 0
        ElseIf a = 1 OrElse a = 2 Then
            Return 1
        ElseIf a = 3 OrElse a = 4 OrElse a = 5 Then
            Return 1
        End If
    End Function
End Class");
        }

        [Fact]
        public void TestBreak()
        {
            Test<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        $Select Case a
            Case 0
                Dim b As Integer = 1
                Exit Select
            Case 1, 2
                Exit Select
            Case 3, 4, 5
                Exit Select
            Case Else
                Exit Select
        End Select
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        If a = 0 Then
            Dim b As Integer = 1
        ElseIf a = 1 OrElse a = 2 Then
        ElseIf a = 3 OrElse a = 4 OrElse a = 5 Then
        Else
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestOperatorPriority()
        {
            Test<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        $Select Case a
            Case 0
                Return 0
            Case If(1 = 1, 1, 2)
                Return 1
            Case Else
                Return 2
        End Select
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        If a = 0 Then
            Return 0
        ElseIf a = If(1 = 1, 1, 2) Then
            Return 1
        Else
            Return 2
        End If
    End Function
End Class");
        }

        [Fact]
        public void TestEmptySwitch()
        {
            TestWrongContext<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        $Select Case a
        End Select
    End Sub
End Class");
        }

        [Fact]
        public void TestSwitchWithDefaultOnly()
        {
            TestWrongContext<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        $Select Case a
            Case Else
        End Select
    End Sub
End Class");
        }

        [Fact]
        public void TestNonTrailingBreak()
        {
            TestWrongContext<ConvertSelectCaseToIfCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer, b As Integer)
        $Select Case a
            Case 0
                If b = 0 Then
                    Exit Select
                End If
                b = 1
                Exit Select
            Case Else
                Exit Select
        End Select
    End Sub
End Class");
        }

    }
}
