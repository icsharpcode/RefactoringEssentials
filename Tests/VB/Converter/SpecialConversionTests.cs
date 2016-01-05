using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactoringEssentials.Tests.VB.Converter
{
    [TestFixture]
    public class SpecialConversionTests : ConverterTestBase
    {
        [Test]
        public void TestSimpleInlineAssign()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    void TestMethod()
    {
        int a, b;
        b = a = 5;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim a, b As Integer
        b = __InlineAssignHelper(a, 5)
    End Sub

    <Obsolete(""Please refactor code that uses this function, it is a simple work-around to simulate inline assignment in VB!"")>
    Private Shared Function __InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class");
        }

        [Test]
        public void TestSimplePostIncrementAssign()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    void TestMethod()
    {
        int a = 5, b;
        b = a++;
    }
}", @"Class TestClass
    Sub TestMethod()
        Dim b As Integer, a As Integer = 5
        b = Math.Min(System.Threading.Interlocked.Increment(a), a - 1)
    End Sub
End Class");
        }
    }
}
