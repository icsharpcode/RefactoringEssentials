using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ConvertIfStatementToSelectCaseStatementTests : VBCodeRefactoringTestBase
    {

        [Fact]
        public void TestBreak()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        $If a = 0 Then
            b = 0
        ElseIf a = 1 Then
            b = 1
        ElseIf a = 2 OrElse a = 3 Then
            b = 2
        Else
            b = 3
        End If
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        Select Case a
            Case 0
                b = 0
            Case 1
                b = 1
            Case 2, 3
                b = 2
            Case Else
                b = 3
        End Select
    End Sub
End Class");
        }

        [Fact]
        public void TestBreakWithComment()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        ' Some comment
        $If a = 0 Then
            b = 0
        ElseIf a = 1 Then
            b = 1
        ElseIf a = 2 OrElse a = 3 Then
            b = 2
        Else
            b = 3
        End If
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        ' Some comment
        Select Case a
            Case 0
                b = 0
            Case 1
                b = 1
            Case 2, 3
                b = 2
            Case Else
                b = 3
        End Select
    End Sub
End Class");
        }

        [Fact]
        public void TestReturn()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        $If a = 0 Then
            Dim b As Integer = 1
            Return b + 1
        ElseIf a = 2 OrElse a = 3 Then
            Return 2
        Else
            Return -1
        End If
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        Select Case a
            Case 0
                Dim b As Integer = 1
                Return b + 1
            Case 2, 3
                Return 2
            Case Else
                Return -1
        End Select
    End Function
End Class");
        }

        [Fact]
        public void TestConstantExpression()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As System.Nullable(Of Integer)) As Integer
        $If a = (If(1 = 1, 11, 12)) Then
            Return 1
        ElseIf a = (2 * 3) + 1 OrElse a = 6 \ 2 Then
            Return 2
        ElseIf a = CInt(10L + 2) OrElse a = 0 OrElse a = 4 Then
            Return 3
        Else
            Return -1
        End If
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As System.Nullable(Of Integer)) As Integer
        Select Case a
            Case (If(1 = 1, 11, 12))
                Return 1
            Case (2 * 3) + 1, 6 \ 2
                Return 2
            Case CInt(10L + 2), 0, 4
                Return 3
            Case Else
                Return -1
        End Select
    End Function
End Class");


            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Const b As Integer = 0
    Private Function TestMethod(a As Integer) As Integer
        Const  c As Integer = 1
        $If a = b Then
            Return 1
        ElseIf a = b + c Then
            Return 0
        Else
            Return -1
        End If
    End Function
End Class", @"
Class TestClass
    Const b As Integer = 0
    Private Function TestMethod(a As Integer) As Integer
        Const  c As Integer = 1
        Select Case a
            Case b
                Return 1
            Case b + c
                Return 0
            Case Else
                Return -1
        End Select
    End Function
End Class");
        }

        [Fact]
        public void TestNestedOr()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        $If a = 0 Then
            Return 1
        ElseIf (a = 2 OrElse a = 4) OrElse (a = 3 OrElse a = 5) Then
            Return 2
        Else
            Return -1
        End If
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As Integer) As Integer
        Select Case a
            Case 0
                Return 1
            Case 2, 4, 3, 5
                Return 2
            Case Else
                Return -1
        End Select
    End Function
End Class");
        }

        [Fact]
        public void TestComplexSwitchExpression()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As Integer, b As Integer) As Integer
        $If a + b = 0 Then
            Return 1
        ElseIf 1 = a + b Then
            Return 0
        Else
            Return -1
        End If
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As Integer, b As Integer) As Integer
        Select Case a + b
            Case 0
                Return 1
            Case 1
                Return 0
            Case Else
                Return -1
        End Select
    End Function
End Class");
        }

        [Fact]
        public void TestNonConstantExpression()
        {
            TestWrongContext<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer, c As Integer)
        Dim b As Integer
        $If a = 0 Then
            b = 0
        ElseIf a = c Then
            b = 1
        ElseIf a = 2 OrElse a = 3 Then
            b = 2
        Else
            b = 3
        End If
    End Sub
End Class
");
            TestWrongContext<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer, c As Integer)
        Dim b As Integer
        $If a = c Then
            b = 0
        ElseIf a = 1 Then
            b = 1
        ElseIf a = 2 OrElse a = 3 Then
            b = 2
        Else
            b = 3
        End If
    End Sub
End Class");
            TestWrongContext<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer, c As Integer)
        Dim b As Integer
        $If a = 0 Then
            b = 0
        ElseIf a = 1 Then
            b = 1
        ElseIf a = 2 OrElse a = c Then
            b = 2
        Else
            b = 3
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestNonEqualityComparison()
        {
            TestWrongContext<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        $If a = 0 Then
            b = 0
        ElseIf a > 4 Then
            b = 1
        ElseIf a = 2 OrElse a = 3 Then
            b = 2
        Else
            b = 3
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestValidType()
        {
            // enum
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Enum TestEnum
    First
    Second
End Enum
Class TestClass
    Private Function TestMethod(a As TestEnum) As Integer
        $If a = TestEnum.First Then
            Return 1
        Else
            Return -1
        End If
    End Function
End Class", @"
Enum TestEnum
    First
    Second
End Enum
Class TestClass
    Private Function TestMethod(a As TestEnum) As Integer
        Select Case a
            Case TestEnum.First
                Return 1
            Case Else
                Return -1
        End Select
    End Function
End Class");

            TestValidType("String", "\"test\"");
            TestValidType("Boolean", "True");
            TestValidType("Char", "\"a\"C");
            TestValidType("Byte", "0");
            TestValidType("SByte", "0");
            TestValidType("Short", "0");
            TestValidType("Long", "0");
            TestValidType("UShort", "0");
            TestValidType("UInteger", "0");
            TestValidType("ULong", "0");
            TestValidType("Boolean?", "Nothing");
        }

        void TestValidType(string type, string caseValue)
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Function TestMethod(a As " + type + @") As Integer
        $If a = " + caseValue + @" Then
            Return 1
        Else
            Return -1
        End If
    End Function
End Class", @"
Class TestClass
    Private Function TestMethod(a As " + type + @") As Integer
        Select Case a
            Case " + caseValue + @"
                Return 1
            Case Else
                Return -1
        End Select
    End Function
End Class");
        }

        [Fact]
        public void TestInvalidType()
        {
            TestWrongContext<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Double)
        Dim b As Integer
        $If a = 0 Then
            b = 0
        Else
            b = 3
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestNoElse()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        $If a = 0 Then
            b = 0
        ElseIf a = 1 Then
            b = 1
        End If
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        Select Case a
            Case 0
                b = 0
            Case 1
                b = 1
        End Select
    End Sub
End Class");
        }

        [Fact]
        public void TestNestedIf()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        $If a = 0 Then
            If b = 0 Then
                Return
            End If
        ElseIf a = 2 OrElse a = 3 Then
            b = 2
        End If
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        Select Case a
            Case 0
                If b = 0 Then
                    Return
                End If

            Case 2, 3
                b = 2
        End Select
    End Sub
End Class");
        }

        [Fact]
        public void TestInLoop()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        While True
            $If a = 0 Then
                b = 0
            ElseIf a = 1 Then
                b = 1
            ElseIf a = 2 OrElse a = 3 Then
                b = 2
            Else
                Exit While
            End If
        End While
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        While True
            Select Case a
                Case 0
                    b = 0
                Case 1
                    b = 1
                Case 2, 3
                    b = 2
                Case Else
                    Exit While
            End Select
        End While
    End Sub
End Class");
        }
        [Fact]
        public void TestInLoop2()
        {
            Test<ConvertIfStatementToSelectCaseStatementCodeRefactoringProvider>(@"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        While True
            $If a = 0 Then
                Exit While
            ElseIf a = 1 Then
                b = 1
            Else
                b = 2
            End If
        End While
    End Sub
End Class", @"
Class TestClass
    Private Sub TestMethod(a As Integer)
        Dim b As Integer
        While True
            Select Case a
                Case 0
                    Exit While
                Case 1
                    b = 1
                Case Else
                    b = 2
            End Select
        End While
    End Sub
End Class");
        }

    }
}
