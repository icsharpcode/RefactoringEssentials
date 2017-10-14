using RefactoringEssentials.VB;
using RefactoringEssentials.Tests.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB
{
	/// <summary>
	/// Tests for AddCheckForNothingCodeRefactoringProvider.
	/// </summary>
    public class AddCheckForNothingTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void TestSingleExpression()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine($str)
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        If str IsNot Nothing Then
            Console.WriteLine(str)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestValueType()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(dateTime As DateTime)
        Console.WriteLine($dateTime)
    End Sub
End Class");
        }

        [Fact]
        public void TestNullableType()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(dateTime As DateTime?)
        Console.WriteLine($dateTime)
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(dateTime As DateTime?)
        If dateTime IsNot Nothing Then
            Console.WriteLine(dateTime)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestMemberAccessExpression1()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class SomeData
    Public Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        Console.WriteLine(data.$Name)
    End Sub
End Class", @"
Imports System

Class SomeData
    Public Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        If data.Name IsNot Nothing Then
            Console.WriteLine(data.Name)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestMemberAccessExpression2()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class SomeData
    Public Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        Console.WriteLine($data.Name)
    End Sub
End Class", @"
Imports System

Class SomeData
    Public Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        If data IsNot Nothing Then
            Console.WriteLine(data.Name)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestMemberAccessExpression3()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class SomeData
    Public Property SubData As SomeSubData
End Class

Class SomeSubData
    Public Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        Console.WriteLine(data.$SubData.Name)
    End Sub
End Class", @"
Imports System

Class SomeData
    Public Property SubData As SomeSubData
End Class

Class SomeSubData
    Public Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        If data.SubData IsNot Nothing Then
            Console.WriteLine(data.SubData.Name)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestStaticMemberAccessExpression()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class SomeData
    Public Shared Property Name As String
End Class

Class TestClass
    Public Sub TestMethod()
        Console.WriteLine($SomeData.Name)
    End Sub
End Class");
        }

        [Fact]
        public void TestMethodInvocation()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class SomeData
    Public Function Output() As String
    End Function
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        $data.Output()
    End Sub
End Class", @"
Imports System

Class SomeData
    Public Function Output() As String
    End Function
End Class

Class TestClass
    Public Sub TestMethod()
        Dim data As New SomeData
        If data IsNot Nothing Then
            data.Output()
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestStaticMethodInvocation()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class SomeData
    Public Shared Function Output() As String
    End Function
End Class

Class TestClass
    Public Sub TestMethod()
        $SomeData.Output()
    End Sub
End Class");
        }

        [Fact]
        public void TestIndexerAccess()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine($str(0))
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        If str IsNot Nothing Then
            Console.WriteLine(str(0))
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestMultipleUsage()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine($str)
        Dim str2 As String = str.ToLower()
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        If str IsNot Nothing Then
            Console.WriteLine(str)
        End If
        Dim str2 As String = str.ToLower()
    End Sub
End Class");
        }

        [Fact]
        public void TestLocalVariable()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine(str)
        Dim str2 As String = str.ToLower()
        Console.WriteLine($str2)
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine(str)
        Dim str2 As String = str.ToLower()
        If str2 IsNot Nothing Then
            Console.WriteLine(str2)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestLocalVariableDeclaration()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine(str)
        Dim $str2 As String = str.ToLower()
        Console.WriteLine(str2)
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInLocalVariableDeclaration()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        Console.WriteLine(str)
        Dim str2 As String = $str.ToLower()
        Console.WriteLine(str2)
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInIfCondition()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        If $list.Contains(""Bla"") Then
            Console.WriteLine(""Contains 'Bla'"")
        End If
    End Sub
End Class", @"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        If (list IsNot Nothing) AndAlso list.Contains(""Bla"") Then
            Console.WriteLine(""Contains 'Bla'"")
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInIfBlock()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(i As Integer, str As String)
        If i > 0 Then
            Console.WriteLine($str)
        End If
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(i As Integer, str As String)
        If (str IsNot Nothing) AndAlso (i > 0) Then
            Console.WriteLine(str)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInElseIfBlock()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(i As Integer, str As String)
        If i > 0 Then
            Console.WriteLine(""i > 0"")
        Else If i = 0 Then
            Console.WriteLine($str)
        End If
    End Sub
End Class", @"
Imports System

Class TestClass
    Public Sub TestMethod(i As Integer, str As String)
        If i > 0 Then
            Console.WriteLine(""i > 0"")
        Else If (str IsNot Nothing) AndAlso (i = 0) Then
            Console.WriteLine(str)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInForeachLoop()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        For Each item In $list
            Console.WriteLine(item)
        Next
    End Sub
End Class", @"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        If list IsNot Nothing Then

            For Each item In list
                Console.WriteLine(item)
            Next
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInWhileLoop()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IList(Of String))
        While $list.Count > 0
            Console.WriteLine(item)
            list.RemoveAt(0)
        End While
    End Sub
End Class", @"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IList(Of String))
        If list IsNot Nothing Then

            While list.Count > 0
                Console.WriteLine(item)
                list.RemoveAt(0)
            End While
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInLambda1()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Class TestClass
    Public Sub TestMethod(list As IList(Of String))
        Dim lambda = Sub(list) Console.WriteLine($list.FirstOrDefault())
    End Sub
End Class", @"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Class TestClass
    Public Sub TestMethod(list As IList(Of String))
        Dim lambda = Sub(list) If list IsNot Nothing Then Console.WriteLine(list.FirstOrDefault())
    End Sub
End Class");
        }

[Fact]
        public void TestUsageInLambda2()
        {
            Test<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Class TestClass
    Public Sub TestMethod(list As IList(Of String))
        Dim lambda = Sub(list)
                         Console.WriteLine($list.FirstOrDefault())
                     End Sub
    End Sub
End Class", @"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Class TestClass
    Public Sub TestMethod(list As IList(Of String))
        Dim lambda = Sub(list)
                         If list IsNot Nothing Then
                             Console.WriteLine(list.FirstOrDefault())
                         End If
                     End Sub
    End Sub
End Class");
        }

        [Fact]
        public void TestUsageInReturnStatement()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Class TestClass
    Public Function TestMethod(list As IList(Of String)) As String
        Return $list.FirstOrDefault()
    End Function
End Class");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck1()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        If str IsNot Nothing Then
            Console.WriteLine($str)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck2()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        If (list IsNot Nothing) AndAlso list.Contains(""Bla"") Then
            Console.WriteLine($list.First())
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck3()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        If ($list IsNot Nothing) AndAlso list.Contains(""Bla"") Then
            Console.WriteLine(list.First())
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck4()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        If (list IsNot Nothing) AndAlso $list.Contains(""Bla"") Then
            Console.WriteLine(list.First())
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck5()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        If str IsNot Nothing Then Console.WriteLine($str)
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck6()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        If str = ""Something"" Then
            Console.WriteLine(""Something!!!"")
        Else If str IsNot Nothing Then
            Console.WriteLine($str)
        End If
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentNotNullCheckInWhileLoop()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System

Class TestClass
    Public Sub TestMethod(str As String)
        ' Yes, this is an infinite loop
        While str IsNot Nothing
            Console.WriteLine($str)
        End While
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentNullCheckInConditionalTernaryExpression1()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        Console.WriteLine(If(list IsNot Nothing, $list.First(), ""))
    End Sub
End Class");
        }

        [Fact]
        public void TestAlreadyPresentNullCheckInConditionalTernaryExpression2()
        {
            TestWrongContext<AddCheckForNothingCodeRefactoringProvider>(@"
Imports System
Imports System.Collections.Generic

Class TestClass
    Public Sub TestMethod(list As IEnumerable(Of String))
        Console.WriteLine(If($list IsNot Nothing, list.First(), ""))
    End Sub
End Class");
        }
    }
}

