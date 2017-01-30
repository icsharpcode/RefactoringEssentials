using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class InvertIfTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimple()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Sub Test()
        $If a = b Then
            Case1()
        Else
            Case2()
        End If
    End Sub
End Class", @"
Class TestClass
    Sub Test()
        If a <> b Then
            Case2()
        Else
            Case1()
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestConditions()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Sub Test()
        $If (a = b AndAlso c > d) OrElse f <= g Or h Then
            Case1()
        Else
            Case2()
        End If
    End Sub
End Class", @"
Class TestClass
    Sub Test()
        If (a <> b OrElse c <= d) AndAlso f > g And Not h Then
            Case2()
        Else
            Case1()
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestReturn()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Sub Test()
        $If True Then
            Case1()
        End If
    End Sub
End Class", @"
Class TestClass
    Sub Test()
        If False Then
            Return
        End If
        Case1()
    End Sub
End Class");
        }

        [Fact]
        public void TestInLoop()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Sub Test()
        Do While True
            $If True Then
                Case1()
            End If
        Loop
    End Sub
End Class", @"
Class TestClass
    Sub Test()
        Do While True
            If False Then
                Continue Do
            End If
            Case1()
        Loop
    End Sub
End Class");
        }

        [Fact]
        public void Test2()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Sub Test()
        $If True Then
            Case1()
            Case2()
        Else
            Return
        End If
    End Sub
End Class", @"
Class TestClass
    Sub Test()
        If False Then
            Return
        End If
        Case1()
        Case2()
    End Sub
End Class");
        }

        [Fact]
        public void TestNonVoidMoreComplexMethod()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Function Test() As Integer
        $If True Then
            Case1()
        Else
            Return 0
            testDummyCode()
        End If
    End Function
End Class", @"
Class TestClass
    Function Test() As Integer
        If False Then
            Return 0
            testDummyCode()
        End If
        Case1()
    End Function
End Class");
        }

        [Fact]
        public void TestComment()
        {
            Test<InvertIfCodeRefactoringProvider>(@"
Class TestClass
    Function Test() As Integer
        $If True Then
            Case1()
        Else
            'TestComment
            Return 0
        End If
    End Function
End Class", @"
Class TestClass
    Function Test() As Integer
        If False Then
            'TestComment
            Return 0
        End If
        Case1()
    End Function
End Class");
        }

    }
}
