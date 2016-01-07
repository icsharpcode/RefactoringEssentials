
using NUnit.Framework;

namespace RefactoringEssentials.Tests.VB.Converter
{
    [TestFixture]
    public class NamespaceLevelTests : ConverterTestBase
    {
        [Test]
        public void TestNamespace()
        {
            TestConversionCSharpToVisualBasic(@"namespace Test
{
    
}", @"Namespace Test
End Namespace");
        }

        [Test]
        public void TestTopLevelAttribute()
        {
            TestConversionCSharpToVisualBasic(
                @"[assembly: CLSCompliant(true)]",
                @"<Assembly: CLSCompliant(True)>");
        }

        [Test]
        public void TestImports()
        {
            TestConversionCSharpToVisualBasic(
                @"using System;
using VB = Microsoft.VisualBasic;",
                @"Imports System
Imports VB = Microsoft.VisualBasic");
        }

        [Test]
        public void TestClass()
        {
            TestConversionCSharpToVisualBasic(@"namespace Test.@class
{
    class TestClass<T>
    {
    }
}", @"Namespace Test.[class]
    Class TestClass(Of T)
    End Class
End Namespace");
        }

        [Test]
        public void TestInternalStaticClass()
        {
            TestConversionCSharpToVisualBasic(@"namespace Test.@class
{
    internal static class TestClass
    {
    }
}", @"Namespace Test.[class]
    Friend Shared Class TestClass
    End Class
End Namespace");
        }

        [Test]
        public void TestAbstractClass()
        {
            TestConversionCSharpToVisualBasic(@"namespace Test.@class
{
    abstract class TestClass
    {
    }
}", @"Namespace Test.[class]
    MustInherit Class TestClass
    End Class
End Namespace");
        }

        [Test]
        public void TestSealedClass()
        {
            TestConversionCSharpToVisualBasic(@"namespace Test.@class
{
    sealed class TestClass
    {
    }
}", @"Namespace Test.[class]
    NotInheritable Class TestClass
    End Class
End Namespace");
        }

        [Test]
        public void TestInterface()
        {
            TestConversionCSharpToVisualBasic(
                @"interface ITest : System.IDisposable
{
    void Test ();
}", @"Interface ITest
    Inherits System.IDisposable

    Sub Test()
End Interface");
        }

        [Test]
        public void TestEnum()
        {
            TestConversionCSharpToVisualBasic(
    @"internal enum ExceptionResource
{
    Argument_ImplementIComparable,
    ArgumentOutOfRange_NeedNonNegNum,
    ArgumentOutOfRange_NeedNonNegNumRequired,
    Arg_ArrayPlusOffTooSmall
}", @"Friend Enum ExceptionResource
    Argument_ImplementIComparable
    ArgumentOutOfRange_NeedNonNegNum
    ArgumentOutOfRange_NeedNonNegNumRequired
    Arg_ArrayPlusOffTooSmall
End Enum");
        }

        [Test]
        public void TestClassInheritanceList()
        {
            TestConversionCSharpToVisualBasic(
    @"abstract class ClassA : System.IDisposable
{
    abstract void Test();
}", @"MustInherit Class ClassA
    Implements System.IDisposable

    MustOverride Sub Test()
End Class");

            TestConversionCSharpToVisualBasic(
                @"abstract class ClassA : System.EventArgs, System.IDisposable
{
    abstract void Test();
}", @"MustInherit Class ClassA
    Inherits System.EventArgs
    Implements System.IDisposable

    MustOverride Sub Test()
End Class");
        }

        [Test]
        public void TestStruct()
        {
            TestConversionCSharpToVisualBasic(
    @"struct MyType : System.IComparable<MyType>
{
    void Test() {}
}", @"Structure MyType
    Implements System.IComparable(Of MyType)

    Sub Test()
    End Sub
End Structure");
        }

        [Test]
        public void TestDelegate()
        {
            TestConversionCSharpToVisualBasic(
                @"public delegate void Test();", 
                @"Public Delegate Sub Test()");
            TestConversionCSharpToVisualBasic(
                @"public delegate int Test();",
                @"Public Delegate Function Test() As Integer");
            TestConversionCSharpToVisualBasic(
                @"public delegate void Test(int x);",
                @"Public Delegate Sub Test(ByVal x As Integer)");
            TestConversionCSharpToVisualBasic(
                @"public delegate void Test(ref int x);",
                @"Public Delegate Sub Test(ByRef x As Integer)");
        }
    }
}
