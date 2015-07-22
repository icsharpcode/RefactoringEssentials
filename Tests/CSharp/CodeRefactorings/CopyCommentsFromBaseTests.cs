using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CopyCommentsFromBaseTest : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestCopyMethodMultiString()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
namespace TestNS
{
    class TestClass
    {
        ///<summary>ssss
        ///ssss</summary>
        public virtual void Test()
        {
            int a;
        }
    }
    class DerivdedClass : TestClass
    {
        public override void $Test()
        {
            string str = string.Empty;
        }
    }
}", @"
namespace TestNS
{
    class TestClass
    {
        ///<summary>ssss
        ///ssss</summary>
        public virtual void Test()
        {
            int a;
        }
    }
    class DerivdedClass : TestClass
    {
        /// <summary>ssss
        /// ssss</summary>
        public override void Test()
        {
            string str = string.Empty;
        }
    }
}");
        }

        [Test]
        public void TestCopyMethodSingleString()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
namespace TestNS
{
    class TestClass
    {
        ///ssss
        public virtual void Test()
        {
            int a;
        }
    }
    class DerivdedClass : TestClass
    {
        public override void $Test()
        {
            string str = string.Empty;
        }
    }
}", @"
namespace TestNS
{
    class TestClass
    {
        ///ssss
        public virtual void Test()
        {
            int a;
        }
    }
    class DerivdedClass : TestClass
    {
        /// ssss
        public override void Test()
        {
            string str = string.Empty;
        }
    }
}");
        }

        [Test]
        public void TestCopyMethodAbstractClassString()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
namespace TestNS
{
    abstract class TestClass
    {
        ///ssss
        ///ssss
        public abstract void Test();
    }
    class DerivdedClass : TestClass
    {
        public override void $Test()
        {
            string str = string.Empty;
        }
    }
}", @"
namespace TestNS
{
    abstract class TestClass
    {
        ///ssss
        ///ssss
        public abstract void Test();
    }
    class DerivdedClass : TestClass
    {
        /// ssss
        /// ssss
        public override void Test()
        {
            string str = string.Empty;
        }
    }
}");
        }


        [Test]
        public void TestCopyProperty()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
namespace TestNS
{
    class TestClass
    {
        /// <summary>
        /// FooBar
        /// </summary>
        public virtual int Test { get; set; }
    }
    class DerivdedClass : TestClass
    {
        public override int $Test { get; set; }
    }
}", @"
namespace TestNS
{
    class TestClass
    {
        /// <summary>
        /// FooBar
        /// </summary>
        public virtual int Test { get; set; }
    }
    class DerivdedClass : TestClass
    {
        /// <summary>
        /// FooBar
        /// </summary>
        public override int Test { get; set; }
    }
}");
        }

        [Test]
        public void TestCopyType()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
/// <summary>
/// FooBar
/// </summary>
class Base 
{
}

class $TestClass : Base
{
}
", @"
/// <summary>
/// FooBar
/// </summary>
class Base 
{
}

/// <summary>
/// FooBar
/// </summary>
class TestClass : Base
{
}
");
        }


        [Test]
        public void TestSkipExisting()
        {
            TestWrongContext<CopyCommentsFromBaseCodeRefactoringProvider>(@"
/// <summary>
/// FooBar
/// </summary>
class Base 
{
}

/// <summary>
/// FooBar
/// </summary>
class $TestClass : Base
{
}
");
        }

        [Test]
        public void TestSkipEmpty()
        {
            TestWrongContext<CopyCommentsFromBaseCodeRefactoringProvider>(@"
class Base 
{
}

class $TestClass : Base
{
}
");
        }



        [Test]
        public void TestInterfaceSimpleCase()
        {
            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
interface ITest
{
    ///sssss
    void Method ();
}
class TestClass : ITest
{
    public void $Method ()
    {
    }
}", @"
interface ITest
{
    ///sssss
    void Method ();
}
class TestClass : ITest
{
    /// sssss
    public void Method ()
    {
    }
}");
        }

        [Test]
        public void TestInterfaceMultiCase()
        {
            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
interface ITest
{
    ///sssss
    ///sssss
    void Method ();
}
class TestClass : ITest
{
    public void $Method ()
    {
    }
}", @"
interface ITest
{
    ///sssss
    ///sssss
    void Method ();
}
class TestClass : ITest
{
    /// sssss
    /// sssss
    public void Method ()
    {
    }
}");
        }

        [Ignore]
        public void TestInterfaceNoProblem()
        {
            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    public void $Method ()
    {
    }
}", @"
interface ITest
{
    void Method ();
}
class TestClass : ITest
{
    public void Method ()
    {
    }
}");
        }

    }
}