using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
    public void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3) where T : class, new where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        argument3 = default(T3);
    }
}", @"Class TestClass
    Public Sub TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
        argument = Nothing
        argument2 = Nothing
        argument3 = Nothing
    End Sub
End Class");
        }

        [Test]
        public void TestMethodWithReturnType()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public int TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3) where T : class, new where T2 : struct
    {
        return 0;
    }
}", @"Class TestClass
    Public Function TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3) As Integer
        Return 0
    End Function
End Class");
        }

        [Test]
        public void TestStaticMethod()
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
        public void TestAbstractMethod()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public abstract void TestMethod();
}", @"Class TestClass
    Public MustOverride Sub TestMethod()
End Class");
        }

        [Test]
        public void TestSealedMethod()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public sealed void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3) where T : class, new where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        argument3 = default(T3);
    }
}", @"Class TestClass
    Public NotOverridable Sub TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
        argument = Nothing
        argument2 = Nothing
        argument3 = Nothing
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Extensions methods not implemented yet.")]
#endif
        public void TestExtensionMethod()
        {
            TestConversionCSharpToVisualBasic(
                @"static class TestClass
{
    public static void TestMethod(this String str)
    {
    }
}", @"Imports System.Runtime.CompilerServices

Module TestClass
    <Extension()> 
    Public Sub TestMethod(ByVal str As String)
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Extensions methods not implemented yet.")]
#endif
        public void TestExtensionMethodWithExistingImport()
        {
            TestConversionCSharpToVisualBasic(
                @"using System.Runtime.CompilerServices;

static class TestClass
{
    public static void TestMethod(this String str)
    {
    }
}", @"Imports System.Runtime.CompilerServices

Module TestClass
    <Extension()> 
    Public Sub TestMethod(ByVal str As String)
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
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void TestConstructor()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass<T, T2, T3> where T : class, new where T2 : struct
{
    public TestClass(out T argument, ref T2 argument2, T3 argument3)
    {
    }
}", @"Class TestClass(Of T As {Class, New}, T2 As Structure, T3)
    Public Sub New(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void TestDestructor()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    ~TestClass()
    {
    }
}", @"Class TestClass
    Protected Overrides Sub Finalize()
    End Sub
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Not implemented yet")]
#endif
        public void TestEvent()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public event EventHandler MyEvent;
}", @"Class TestClass
    Public Event MyEvent As EventHandler
End Class");
        }

        [Test]
#if !UNIMPLEMENTED_CONVERTER_FEATURE_TESTS
        [Ignore("Indexer support not implemented yet.")]
#endif
        public void TestIndexer()
        {
            TestConversionCSharpToVisualBasic(
                @"class TestClass
{
    public int this[int index] { get; set; }
    public int this[string index] {
        get { return 0; }
    }
    int m_test3;
    public int this[double index] {
        get { return this.m_test3; }
        set { this.m_test3 = value; }
    }
}", @"Class TestClass
    Default Public Property Item(ByVal index As Integer) As Integer

    Default Public Property Item(ByVal index As String) As Integer
        Get
            Return 0
        End Get
    End Property

    Private m_test3 As Integer

    Default Public Property Item(ByVal index As Double) As Integer
        Get
            Return Me.m_test3
        End Get
        Set(value As Integer)
            Me.m_test3 = value
        End Set
    End Property
End Class");
        }
    }
}