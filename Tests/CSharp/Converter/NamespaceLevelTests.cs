
using NUnit.Framework;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
    [TestFixture]
    public class NamespaceLevelTests : ConverterTestBase
    {
        [Test]
        public void TestNamespace()
        {
            TestConversionVisualBasicToCSharp(@"Namespace Test
End Namespace", @"namespace Test
{
    
}");
        }

        [Test]
        public void TestTopLevelAttribute()
        {
            TestConversionVisualBasicToCSharp(
				@"<Assembly: CLSCompliant(True)>",
				@"[assembly: CLSCompliant(true)]");
        }

        [Test]
        public void TestImports()
        {
            TestConversionVisualBasicToCSharp(
				@"Imports SomeNamespace
Imports VB = Microsoft.VisualBasic",
				@"using SomeNamespace;
using VB = Microsoft.VisualBasic;");
        }

        [Test]
        public void TestClass()
        {
            TestConversionVisualBasicToCSharp(@"Namespace Test.[class]
    Class TestClass(Of T)
    End Class
End Namespace", @"namespace Test.@class
{
    class TestClass<T>
    {
    }
}");
        }

        [Test]
        public void TestInternalStaticClass()
        {
            TestConversionVisualBasicToCSharp(@"Namespace Test.[class]
    Friend Module TestClass
        Sub Test()
        End Sub

        Private Sub Test2()
        End Sub
    End Module
End Namespace", @"namespace Test.@class
{
    internal static class TestClass
    {
        public static void Test() {}
        static void Test2() {}
    }
}");
        }

        [Test]
        public void TestAbstractClass()
        {
            TestConversionVisualBasicToCSharp(@"Namespace Test.[class]
    MustInherit Class TestClass
    End Class
End Namespace", @"namespace Test.@class
{
    abstract class TestClass
    {
    }
}");
        }

        [Test]
        public void TestSealedClass()
        {
            TestConversionVisualBasicToCSharp(@"Namespace Test.[class]
    NotInheritable Class TestClass
    End Class
End Namespace", @"namespace Test.@class
{
    sealed class TestClass
    {
    }
}");
        }

        [Test]
        public void TestInterface()
        {
            TestConversionVisualBasicToCSharp(
@"Interface ITest
    Inherits System.IDisposable

    Sub Test()
End Interface", @"interface ITest : System.IDisposable
{
    void Test ();
}");
        }

        [Test]
        public void TestEnum()
        {
            TestConversionVisualBasicToCSharp(
@"Friend Enum ExceptionResource
    Argument_ImplementIComparable
    ArgumentOutOfRange_NeedNonNegNum
    ArgumentOutOfRange_NeedNonNegNumRequired
    Arg_ArrayPlusOffTooSmall
End Enum", @"internal enum ExceptionResource
{
    Argument_ImplementIComparable,
    ArgumentOutOfRange_NeedNonNegNum,
    ArgumentOutOfRange_NeedNonNegNumRequired,
    Arg_ArrayPlusOffTooSmall
}");
        }

        [Test]
        public void TestClassInheritanceList()
        {
            TestConversionVisualBasicToCSharp(
@"MustInherit Class ClassA
    Implements System.IDisposable

    Protected MustOverride Sub Test()
End Class", @"abstract class ClassA : System.IDisposable
{
    protected abstract void Test();
}");

            TestConversionVisualBasicToCSharp(
@"MustInherit Class ClassA
    Inherits System.EventArgs
    Implements System.IDisposable

    Protected MustOverride Sub Test()
End Class", @"abstract class ClassA : System.EventArgs, System.IDisposable
{
    protected abstract void Test();
}");
        }

        [Test]
        public void TestStruct()
        {
            TestConversionVisualBasicToCSharp(
@"Structure MyType
    Implements System.IComparable(Of MyType)

    Private Sub Test()
    End Sub
End Structure", @"struct MyType : System.IComparable<MyType>
{
    void Test() {}
}");
        }

        [Test]
        public void TestDelegate()
        {
            TestConversionVisualBasicToCSharp(
				@"Public Delegate Sub Test()",
				@"public delegate void Test();");
            TestConversionVisualBasicToCSharp(
				@"Public Delegate Function Test() As Integer",
				@"public delegate int Test();");
            TestConversionVisualBasicToCSharp(
				@"Public Delegate Sub Test(ByVal x As Integer)",
				@"public delegate void Test(int x);");
            TestConversionVisualBasicToCSharp(
				@"Public Delegate Sub Test(ByRef x As Integer)",
				@"public delegate void Test(ref int x);");
        }

        [Test]
        public void MoveImportsStatement()
        {
            TestConversionVisualBasicToCSharp(@"Imports SomeNamespace

Namespace test
End Namespace",
				"namespace test { using SomeNamespace; }");
        }

        [Test]
        public void ClassImplementsInterface()
        {
            TestConversionVisualBasicToCSharp(@"Class test
    Implements IComparable
End Class",
				"using System; class test : IComparable { }");
        }

        [Test]
        public void ClassImplementsInterface2()
        {
            TestConversionVisualBasicToCSharp(@"Class test
    Implements System.IComparable
End Class",
				"class test : System.IComparable { }");
        }

        [Test]
        public void ClassInheritsClass()
        {
            TestConversionVisualBasicToCSharp(@"Imports System.IO

Class test
    Inherits InvalidDataException
End Class",
				"using System.IO; class test : InvalidDataException { }");
        }

        [Test]
        public void ClassInheritsClass2()
        {
            TestConversionVisualBasicToCSharp(@"Class test
    Inherits System.IO.InvalidDataException
End Class",
				"class test : System.IO.InvalidDataException { }");
        }
    }
}
