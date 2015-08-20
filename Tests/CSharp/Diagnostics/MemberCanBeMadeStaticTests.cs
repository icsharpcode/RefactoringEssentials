/*
using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class MemberCanBeMadeStaticTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestPrivateMethod()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"class TestClass
{
    void $Test$ ()
    {
        int a = 2;
    }
}",
                @"class TestClass
{
    static void Test()
    {
        int a = 2;
    }
}"
            );
        }

        [Test]
        public void TestPrivateMethodPublicSkip()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"class TestClass
        {
            void $Test$ ()
            {
                int a = 2;
            }
        }"
            );
        }

        [Test]
        public void TestPublicMethod()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"class TestClass
{
    public void $Test$ ()
    {
        int a = 2;
    }
}",
                @"class TestClass
{
    public static void Test()
    {
        int a = 2;
    }
}"
            );
        }

        [Test]
        public void TestPublicMethodSkip()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"class TestClass
        {
            public void $Test$ ()
            {
                int a = 2;
            }
        }"
            );
        }

        [Test]
        public void MethodThatCallsInstanceMethodOnParameter()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"class TestClass
{
    string $Test$ (string txt)
    {
        return txt.Trim ();
    }
}",
                @"class TestClass
{
    static string Test(string txt)
    {
        return txt.Trim();
    }
}"
            );
        }

        [Test]
        public void TestWithVirtualFunction()
        {

            var input = @"class TestClass
        {
            public virtual void Test()
            {
                int a = 2;
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestWithInterfaceImplementation()
        {
            var input = @"interface IBase {
            void Test();
        }
        class TestClass : IBase
        {
            public void $Test$ ()
            {
                int a = 2;
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestWithStaticFunction()
        {

            var input = @"class TestClass
        {
            static void Test()
            {
                int a = 2;
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestDoNotWarnOnAttributes()
        {

            var input = @"using System;
        class TestClass
        {
            [Obsolete]
            public void Test()
            {
                int a = 2;
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestDoNotWarnOnEmptyMethod()
        {

            var input = @"using System;
        class TestClass
        {
            public void Test()
            {
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestDoNotWarnOnInterfaceMethod()
        {

            var input = @"using System;
        interface ITestInterface
        {
            void Test();
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestDoNotWarnOnNotImplementedMethod()
        {
            var input = @"using System;
        class TestClass
        {
            public void Test ()
            {
                throw new NotImplementedExceptionIssue();
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestPropertyAccess()
        {
            var input = @"using System;
        class TestClass
        {
            public int Foo { get; set; }
            public void Test ()
            {
                System.Console.WriteLine (Foo);
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void DoNotWarnOnMarshalByRefObject()
        {

            var input = @"class TestClass : System.MarshalByRefObject
        {
            public void Test ()
            {
                int a = 2;
            }
        }";
            Analyze<MemberCanBeMadeStaticAnalyzer>(input);
        }

        [Test]
        public void TestProperty()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
 @"class TestClass
{
    int $Test$ {
        get {
            return 2;
        }
    }
}",
@"class TestClass
{
    static int Test {
        get {
            return 2;
        }
    }
}"
            );
        }


        [Test]
        public void TestCustomEvent()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"using System;

class TestClass
{
    static event EventHandler Foo;

    event EventHandler $Bar$ {
        add { Foo += value; }
        remove { Foo -= value; }
    }
}",
            @"using System;

class TestClass
{
    static event EventHandler Foo;

    static event EventHandler Bar
    {
        add { Foo += value; }
        remove { Foo -= value; }
    }
}"
                );
        }


        [Test]
        public void TestCustomEventOnNotImplemented()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"using System;

        class TestClass
        {
            static event EventHandler Foo;

            event EventHandler Bar {
                add { throw new NotImplementedException (); }
                remove { throw new NotImplementedException (); }
            }
        }"
                );
        }

        [Test]
        public void TestDisable()
        {
            Analyze<MemberCanBeMadeStaticAnalyzer>(
                @"class TestClass
        {
        // ReSharper disable once MemberCanBeMadeStatic.Local
#pragma warning disable " + CSharpDiagnosticIDs.MemberCanBeMadeStaticAnalyzerID + @"
            void Test ()
            {
                int a = 2;
            }
        }");
        }
    }
}
*/