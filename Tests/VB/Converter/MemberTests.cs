using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactoringEssentials.Tests.VB.Converter
{
    [TestFixture]
    public class MemberTests : ConverterTestBase
    {
        [Test]
        public void TestMethod()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public static void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3) where T : class, new where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        argument3 = default(T3);
    }
}", @"Class TestClass
    Public Shared Sub TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
        argument = Nothing
        argument2 = Nothing
        argument3 = Nothing
    End Sub
End Class");
        }

        [Test]
        public void TestProperty()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public int Test { get; set; }
    public int Test2 {
        get { return 0; }
    }
    int m_test3;
    public int Test3 {
        get { return this.m_test3; }
        set { this.m_test3 = value; }
    }
}", @"Class TestClass
    Public Property Test As Integer

    Public Property Test2 As Integer
        Get
            Return 0
        End Get
    End Property

    Private m_test3 As Integer

    Public Property Test3 As Integer
        Get
            Return Me.m_test3
        End Get
        Set(value As Integer)
            Me.m_test3 = value
        End Set
    End Property
End Class");
        }

        [Test]
        public void TestIfStatement()
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
    }
}