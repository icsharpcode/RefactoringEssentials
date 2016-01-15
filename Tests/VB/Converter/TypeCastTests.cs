using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RefactoringEssentials.Tests.VB.Converter
{
    [TestFixture]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
    [Ignore("Casting is mostly not implemented yet")]
#endif
    public class TypeCastTests : ConverterTestBase
    {
        [Test]
        public void CastObjectToInteger()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = 5;
    int i = (int) o;
}
", @"Sub Test()
    Dim o As Object = 5
    Dim i As Integer = CType(o, Integer)
End Sub
");
        }

        [Test]
        public void CastObjectToString()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = ""Test"";
    string s = (string) o;
}
", @"Sub Test()
    Dim o As Object = ""Test""
    Dim s As String = CType(o, String)
End Sub
");
        }

        [Test]
        public void CastObjectToGenericList()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = new System.Collections.Generic.List<int>();
    System.Collections.Generic.List<int> l = (System.Collections.Generic.List<int>) o;
}
", @"Sub Test()
    Dim o As Object = new System.Collections.Generic.List(Of Integer)
    Dim l As System.Collections.Generic.List(Of Integer) = CType(o, System.Collections.Generic.List(Of Integer))
End Sub
");
        }

        [Test]
        public void TryCastObjectToInteger()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = 5;
    System.Nullable<int> i = o as int;
}
", @"Sub Test()
    Dim o As Object = 5
    Dim i As System.Nullable(Of Integer) = TryCast(o, Integer)
End Sub
");
        }

        [Test]
        public void TryCastObjectToGenericList()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = new System.Collections.Generic.List<int>();
    System.Collections.Generic.List<int> l = o as System.Collections.Generic.List<int>;
}
", @"Sub Test()
    Dim o As Object = new System.Collections.Generic.List(Of Integer)
    Dim l As System.Collections.Generic.List(Of Integer) = TryCast(o, System.Collections.Generic.List(Of Integer))
End Sub
");
        }

        [Test]
        public void CastConstantNumberToLong()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = 5L;
}
", @"Sub Test()
    Dim o As Object = 5L
End Sub
");
        }

        [Test]
        public void CastConstantNumberToFloat()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = 5.0f;
}
", @"Sub Test()
    Dim o As Object = 5.0F
End Sub
");
        }

        [Test]
        public void CastConstantNumberToDecimal()
        {
            TestConversionCSharpToVisualBasic(
                @"void Test()
{
    object o = 5.0m;
}
", @"Sub Test()
    Dim o As Object = 5.0D
End Sub
");
        }
    }
}
