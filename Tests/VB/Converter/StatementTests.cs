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
        public void AssignmentStatementWithInitialization()
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
        [Ignore("Not implemented yet")]
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
            b = 1;
        }
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer
        b = 0
        While b = 0
            b = 1
        End While
    End Sub
End Class");
        }

        [Test]
        [Ignore("Not implemented yet")]
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
            b = 1;
        }
        while (b == 0);
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer
        b = 0
        Do
            b = 1
        Loop While b = 0 
    End Sub
End Class");
        }
    }
}
