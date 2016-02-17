using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RefactoringEssentials.Tests.VB.Converter
{
    [TestFixture]
    public class ExpressionTests : ConverterTestBase
    {
        [Test]
        public void ConditionalExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(string str)
    {
        bool result = (str == """") ? true : false;
    }
}", @"Class TestClass
    Sub TestMethod(ByVal str As String)
        Dim result As Boolean = If((str = """"), True, False)
    End Sub
End Class");
        }

        [Test]
        public void NullCoalescingExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(string str)
    {
        Console.WriteLine(str ?? ""<null>"");
    }
}", @"Class TestClass
    Sub TestMethod(ByVal str As String)
        Console.WriteLine(If(str, ""<null>""))
    End Sub
End Class");
        }

        [Test]
        public void MemberAccessAndInvocationExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(string str)
    {
        int length;
        length = str.Length;
        Console.WriteLine(""Test"");
        Console.ReadKey();
    }
}", @"Class TestClass
    Sub TestMethod(ByVal str As String)
        Dim length As Integer
        length = str.Length
        Console.WriteLine(""Test"")
        Console.ReadKey()
    End Sub
End Class");
        }

        [Test]
        public void ElvisOperatorExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(string str)
    {
        int length = str?.Length ?? -1;
        Console.WriteLine(length);
        Console.ReadKey();
    }
}", @"Class TestClass
    Sub TestMethod(ByVal str As String)
        Dim length As Integer = If(str?.Length, -1)
        Console.WriteLine(length)
        Console.ReadKey()
    End Sub
End Class");
        }

        [Test]
        public void ObjectInitializerExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class StudentName
{
    public string LastName, FirstName;
}

class TestClass
{
    void TestMethod(string str)
    {
        StudentName student2 = new StudentName
        {
            FirstName = ""Craig"",
            LastName = ""Playstead"",
        };
    }
}", @"Class StudentName
    Public LastName, FirstName As String
End Class

Class TestClass
    Sub TestMethod(ByVal str As String)
        Dim student2 As StudentName = New StudentName With {.FirstName = ""Craig"", .LastName = ""Playstead""}
    End Sub
End Class");
        }

        [Test]
        public void ObjectInitializerExpression2()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    void TestMethod(string str)
    {
        var student2 = new {
            FirstName = ""Craig"",
            LastName = ""Playstead"",
        };
    }
}", @"Class TestClass
    Sub TestMethod(ByVal str As String)
        Dim student2 = New With {Key .FirstName = ""Craig"", Key .LastName = ""Playstead""}
    End Sub
End Class");
        }

        [Test]
        public void ThisMemberAccessExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass
{
    private int member;

    void TestMethod()
    {
        this.member = 0;
    }
}", @"Class TestClass
    Private member As Integer

    Sub TestMethod()
        Me.member = 0
    End Sub
End Class");
        }

        [Test]
        public void BaseMemberAccessExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class BaseTestClass
{
    public int member;
}

class TestClass : BaseTestClass
{
    void TestMethod()
    {
        base.member = 0;
    }
}", @"Class BaseTestClass
    Public member As Integer
End Class

Class TestClass
    Inherits BaseTestClass

    Sub TestMethod()
        MyBase.member = 0
    End Sub
End Class");
        }

        [Test]
        public void DelegateExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass 
{
    void TestMethod()
    {
        var test = delegate(int a) { return a * 2 };

        test(3);
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim test = Function(ByVal a As Integer) a * 2
        test(3)
    End Sub
End Class");
        }

        [Test]
        public void LambdaBodyExpression()
        {
            TestConversionCSharpToVisualBasic(@"
class TestClass 
{
    void TestMethod()
    {
        var test = a => { return a * 2 };
        var test2 = (a, b) => { if (b > 0) return a / b; return 0; }
        var test3 = (a, b) => a % b;

        test(3);
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim test = Function(a) a * 2
        Dim test2 = Function(a, b)
                        If b > 0 Then Return a / b
                        Return 0
                    End Function

        Dim test3 = Function(a, b) a Mod b
        test(3)
    End Sub
End Class");
        }
    }
}
