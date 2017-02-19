using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class PutInsideUsingTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<PutInsideUsingAction>(@"
interface ITest : System.IDisposable
{
    void Test ();
}
class TestClass
{
    void TestMethod (int i)
    {
        ITest obj $= null;
        obj.Test ();
        int a;
        if (i > 0)
            obj.Test ();
        a = 0;
    }
}", @"
interface ITest : System.IDisposable
{
    void Test ();
}
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        using (ITest obj = null)
        {
            obj.Test();
            if (i > 0)
                obj.Test();
        }

        a = 0;
    }
}");
        }

        [Fact]
        public void TestIDisposable()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod()
    {
        System.IDisposable obj $= null;
        obj.Method();
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        using (System.IDisposable obj = null)
        {
            obj.Method();
        }
    }
}");
        }

        [Fact]
        public void TestTypeParameter()
        {

            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod<T> ()
        where T : System.IDisposable, new()
    {
        T obj $= new T ();
        obj.Method ();
    }
}", @"
class TestClass
{
    void TestMethod<T> ()
        where T : System.IDisposable, new()
    {
        using (T obj = new T())
        {
            obj.Method();
        }
    }
}");
        }

        [Fact]
        public void TestMultipleVariablesDeclaration()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable obj, obj2 $= null, obj3;
        obj2.Method ();
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable obj, obj3;
        using (System.IDisposable obj2 = null)
        {
            obj2.Method();
        }
    }
}");
        }

        [Fact]
        public void TestNullInitializer()
        {
            TestWrongContext<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable $obj;
        obj.Method ();
    }
}");
        }

        [Fact]
        public void TestMoveVariableDeclaration()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable obj $= null;
        int a, b;
        a = b = 0;
        obj.Method ();
        a++;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        int a;
        using (System.IDisposable obj = null)
        {
            int b;
            a = b = 0;
            obj.Method();
        }

        a++;
    }
}");
        }

        [Fact]
        public void TestMoveVariableDeclarationAndConvertInitializationToAssignment()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable obj $= null;
        int a = 10, b;        
        obj.Method ();
        a++;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        int a;
        using (System.IDisposable obj = null)
        {
            int b;
            a = 10;
            obj.Method();
        }

        a++;
    }
}");
        }

        [Fact]
        public void TestRemoveDisposeInvocation()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable obj $= null;
        obj.Method();
        obj.Dispose();
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        using (System.IDisposable obj = null)
        {
            obj.Method();
        }
    }
}");
        }

        [Fact]
        public void TestNotAvailableOnNonDisposableVariable()
        {
            TestWrongContext<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.String obj $= null;
        obj.Method ();
        obj.Dispose();
    }
}");
        }

        [Fact]
        public void TestAllDeclaredVariablesAreUsedInsideUsingBlock()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod()
    {
        System.IDisposable obj $= null;
        int a, b;
        a = b = 0;
        a++;
        obj.Method();
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        using (System.IDisposable obj = null)
        {
            int a, b;
            a = b = 0;
            a++;
            obj.Method();
        }
    }
}");
        }

        [Fact]
        public void TestEmptyUsingBlock()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod()
    {
        System.IDisposable obj = null, $obj2 = null;
        int a, b;
        a = b = 0;
        obj.Method();
        a++;
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        System.IDisposable obj = null;
        using (System.IDisposable obj2 = null)
        {
        }

        int a, b;
        a = b = 0;
        obj.Method();
        a++;
    }
}");
        }

        [Fact]
        public void TestVariableWithComment()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod()
    {
        // This is a comment
        System.IDisposable obj $= null;
        int a, b;
        a = b = 0;
        obj.Method();
        a++;
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        int a;
        // This is a comment
        using (System.IDisposable obj = null)
        {
            int b;
            a = b = 0;
            obj.Method();
        }

        a++;
    }
}");
        }

        [Fact]
        public void TestLastCallInBlockIsStatic()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod()
    {
        System.IDisposable obj $= null;
        int a, b;
        a = b = 0;
        a++;
        System.String.Split('.', obj.Method());
    }
}", @"
class TestClass
{
    void TestMethod()
    {
        using (System.IDisposable obj = null)
        {
            int a, b;
            a = b = 0;
            a++;
            System.String.Split('.', obj.Method());
        }
    }
}");
        }

        /// <summary>
        /// Bug 39041 - NOP refactoring option for using
        /// </summary>
        [Fact]
        public void TestBug39041()
        {
            TestWrongContext<PutInsideUsingAction>(@"
interface ITest : System.IDisposable
{
    void Test ();
}
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        using (ITest obj $= null)
        {
            obj.Test();
            if (i > 0)
                obj.Test();
        }

        a = 0;
    }
}");
        }

        [Fact]
        public void TestChangeVarDeclarationToExplicit()
        {
            Test<PutInsideUsingAction>(@"
class TestClass
{
    void TestMethod ()
    {
        System.IDisposable obj $= null;
        var a = obj.GetHashCode();        
        return a;
    }
}", @"
class TestClass
{
    void TestMethod ()
    {
        int a;
        using (System.IDisposable obj = null)
        {
            a = obj.GetHashCode();
        }

        return a;
    }
}");
        }
    }

   

}
