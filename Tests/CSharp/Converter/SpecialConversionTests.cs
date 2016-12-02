using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
    [TestFixture]
    public class SpecialConversionTests : ConverterTestBase
    {
        [Test]
        public void TestSimpleInlineAssign()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        Dim a, b As Integer
        b = __InlineAssignHelper(a, 5)
    End Sub

    <Obsolete(""Please refactor code that uses this function, it is a simple work-around to simulate inline assignment in VB!"")>
    Private Shared Function __InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class", @"class TestClass
{
    void TestMethod()
    {
        int a, b;
        b = a = 5;
    }
}");
        }

        [Test]
        public void TestSimplePostIncrementAssign()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        Dim b As Integer, a As Integer = 5
        b = Math.Min(System.Threading.Interlocked.Increment(a), a - 1)
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        int a = 5, b;
        b = a++;
    }
}");
        }

        [Test]
        public void RaiseEvent()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Event MyEvent As EventHandler

    Private Sub TestMethod()
        RaiseEvent MyEvent(Me, EventArgs.Empty)
    End Sub
End Class", @"class TestClass
{
    event EventHandler MyEvent;

    void TestMethod()
    {
        if (MyEvent != null) MyEvent(this, EventArgs.Empty);
    }
}");
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        RaiseEvent MyEvent(Me, EventArgs.Empty)
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if ((MyEvent != null)) MyEvent(this, EventArgs.Empty);
    }
}");
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        RaiseEvent MyEvent(Me, EventArgs.Empty)
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (null != MyEvent) { MyEvent(this, EventArgs.Empty); }
    }
}");
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        RaiseEvent MyEvent(Me, EventArgs.Empty)
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (this.MyEvent != null) MyEvent(this, EventArgs.Empty);
    }
}");
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        RaiseEvent MyEvent(Me, EventArgs.Empty)
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (MyEvent != null) this.MyEvent(this, EventArgs.Empty);
    }
}");
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        RaiseEvent MyEvent(Me, EventArgs.Empty)
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if ((this.MyEvent != null)) { this.MyEvent(this, EventArgs.Empty); }
    }
}");
        }

        [Test]
        public void IfStatementSimilarToRaiseEvent()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        If FullImage IsNot Nothing Then DrawImage()
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (FullImage != null) DrawImage();
    }
}");
            // regression test:
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        If FullImage IsNot Nothing Then e.DrawImage()
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (FullImage != null) e.DrawImage();
    }
}");
            // with braces:
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        If FullImage IsNot Nothing Then
            DrawImage()
        End If
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (FullImage != null) { DrawImage(); }
    }
}");
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        If FullImage IsNot Nothing Then
            e.DrawImage()
        End If
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (FullImage != null) { e.DrawImage(); }
    }
}");
            // another bug related to the IfStatement code:
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        If Tiles IsNot Nothing Then

            For Each t As Tile In Tiles
                Me.TileTray.Controls.Remove(t)
            Next
        End If
    End Sub
End Class", @"class TestClass
{
    void TestMethod()
    {
        if (Tiles != null) foreach (Tile t in Tiles) this.TileTray.Controls.Remove(t);
    }
}");
        }
    }
}
