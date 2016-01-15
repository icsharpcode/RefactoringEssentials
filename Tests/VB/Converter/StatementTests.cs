using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RefactoringEssentials.Tests.VB.Converter
{
    [TestFixture]
    public class StatementTests : ConverterTestBase
    {
        [Test]
        public void AssignmentStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int b;
        b = 0;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer
        b = 0
    End Sub
End Class");
        }

        [Test]
        public void AssignmentStatementInDeclaration()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int b = 0;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer = 0
    End Sub
End Class");
        }

        [Test]
        public void AssignmentStatementInVarDeclaration()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        var b = 0;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b = 0
    End Sub
End Class");
        }

        [Test]
        public void ObjectInitializationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        string b;
        b = new string(""test"");
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As String
        b = New String(""test"")
    End Sub
End Class");
        }

        [Test]
        public void ObjectInitializationStatementInDeclaration()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        string b = new string(""test"");
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As String = New String(""test"")
    End Sub
End Class");
        }

        [Test]
        public void ObjectInitializationStatementInVarDeclaration()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        var b = new string(""test"");
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b = New String(""test"")
    End Sub
End Class");
        }

        [Test]
        public void ArrayDeclarationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[] b;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer()
    End Sub
End Class");
        }

        [Test]
        public void ArrayInitializationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[] b = { 1, 2, 3 };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer() = {1, 2, 3}
    End Sub
End Class");
        }

        [Test]
        public void ArrayInitializationStatementInVarDeclaration()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        var b = { 1, 2, 3 };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b = {1, 2, 3}
    End Sub
End Class");
        }

        [Test]
        public void ArrayInitializationStatementWithType()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[] b = new int[] { 1, 2, 3 };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer() = New Integer() {1, 2, 3}
    End Sub
End Class");
        }

        [Test]
        public void ArrayInitializationStatementWithLength()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[] b = new int[3] { 1, 2, 3 };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer() = New Integer(2) {1, 2, 3}
    End Sub
End Class");
        }

        [Test]
        public void MultidimensionalArrayDeclarationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[,] b;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer(,)
    End Sub
End Class");
        }

        [Test]
        public void MultidimensionalArrayInitializationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[,] b = { { 1, 2 }, { 3, 4 } };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer(,) = {{1, 2}, {3, 4}}
    End Sub
End Class");
        }

        [Test]
        public void MultidimensionalArrayInitializationStatementWithType()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[,] b = new int[,] { { 1, 2 }, { 3, 4 } };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer(,) = New Integer(,) {{1, 2}, {3, 4}}
    End Sub
End Class");
        }

        [Test]
        public void MultidimensionalArrayInitializationStatementWithLengths()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[,] b = new int[2, 2] { { 1, 2 }, { 3, 4 } };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer(,) = New Integer(1, 1) {{1, 2}, {3, 4}}
    End Sub
End Class");
        }

        [Test]
        public void JaggedArrayDeclarationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[][] b;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer()()
    End Sub
End Class");
        }

        [Test]
        public void JaggedArrayInitializationStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[][] b = { new int[] { 1, 2 }, new int[] { 3, 4 } };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer()() = {New Integer() {1, 2}, New Integer() {3, 4}}
    End Sub
End Class");
        }

        [Test]
        public void JaggedArrayInitializationStatementWithType()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[][] b = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer()() = New Integer()() {New Integer() {1, 2}, New Integer() {3, 4}}
    End Sub
End Class");
        }

        [Test]
        public void JaggedArrayInitializationStatementWithLength()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int[][] b = new int[2][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer()() = New Integer(1)() {New Integer() {1, 2}, New Integer() {3, 4}}
    End Sub
End Class");
        }

        [Test]
        public void IfStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod (int a)
    {
        int b;
        if (a == 0) {
            b = 0;
        } else if (a == 1) {
            b = 1;
        } else if (a == 2 || a == 3) {
            b = 2;
        } else {
            b = 3;
        }
    }
}", @"Class TestClass
    Sub TestMethod(ByVal a As Integer)
        Dim b As Integer

        If a = 0 Then
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
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void WhileStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int b;
        b = 0;
        while (b == 0)
        {
            if (b == 2)
                continue;
            if (b == 3)
                break;
            b = 1;
        }
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer
        b = 0
        While b = 0
            If b = 2 Then
                Continue While
            End If
            If b = 3 Then
                Exit While
            End If
            b = 1
        End While
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void DoWhileStatement()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod()
    {
        int b;
        b = 0;
        do
        {
            if (b == 2)
                continue;
            if (b == 3)
                break;
            b = 1;
        }
        while (b == 0);
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer
        b = 0
        Do
            If b = 2 Then
                Continue Do
            End If
            If b = 3 Then
                Exit Do
            End If
            b = 1
        Loop While b = 0 
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void ForEachStatementWithExplicitType()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(int[] values)
    {
        foreach (int val in values)
        {
            if (val == 2)
                continue;
            if (val == 3)
                break;
        }
    }
}", @"Class TestClass
    Sub TestMethod(ByVal values As Integer())
        For Each val As Integer In values
            If val = 2 Then
                Continue For
            End If
            If val = 3 Then
                Exit For
            End If
        Next
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void ForEachStatementWithVar()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(int[] values)
    {
        foreach (var val in values)
        {
            if (val == 2)
                continue;
            if (val == 3)
                break;
        }
    }
}", @"Class TestClass
    Sub TestMethod(ByVal values As Integer())
        For Each val In values
            If val = 2 Then
                Continue For
            End If
            If val = 3 Then
                Exit For
            End If
        Next
    End Sub
End Class");
        }
    }
}
