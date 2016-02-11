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
    }
}
