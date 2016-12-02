using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
    [TestFixture]
    public class TypeCastTests : ConverterTestBase
    {
        [Test]
        public void CastObjectToInteger()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = 5
    Dim i As Integer = CInt(o)
End Sub
", @"void Test()
{
    object o = 5;
    int i = (int) o;
}
");
        }

        [Test]
        public void CastObjectToString()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = ""Test""
    Dim s As String = CStr(o)
End Sub
", @"void Test()
{
    object o = ""Test"";
    string s = (string) o;
}
");
        }

        [Test]
        public void CastObjectToGenericList()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = New System.Collections.Generic.List(Of Integer)()
    Dim l As System.Collections.Generic.List(Of Integer) = CType(o, System.Collections.Generic.List(Of Integer))
End Sub
", @"void Test()
{
    object o = new System.Collections.Generic.List<int>();
    System.Collections.Generic.List<int> l = (System.Collections.Generic.List<int>) o;
}
");
        }

        [Test]
        public void TryCastObjectToInteger()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = 5
    Dim i As System.Nullable(Of Integer) = TryCast(o, Integer)
End Sub
", @"void Test()
{
    object o = 5;
    System.Nullable<int> i = o as int;
}
");
        }

        [Test]
        public void TryCastObjectToGenericList()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = New System.Collections.Generic.List(Of Integer)()
    Dim l As System.Collections.Generic.List(Of Integer) = TryCast(o, System.Collections.Generic.List(Of Integer))
End Sub
", @"void Test()
{
    object o = new System.Collections.Generic.List<int>();
    System.Collections.Generic.List<int> l = o as System.Collections.Generic.List<int>;
}
");
        }

        [Test]
        public void CastConstantNumberToLong()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = 5L
End Sub
", @"void Test()
{
    object o = 5L;
}
");
        }

        [Test]
        public void CastConstantNumberToFloat()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = 5F
End Sub
", @"void Test()
{
    object o = 5.0f;
}
");
        }

        [Test]
        public void CastConstantNumberToDecimal()
        {
            TestConversionVisualBasicToCSharp(
@"Private Sub Test()
    Dim o As Object = 5.0D
End Sub
", @"void Test()
{
    object o = 5.0m;
}
");
        }
    }
}
