using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EmptyConstructorTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInspectorCase1()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {private int member; $public Test(){}$}", @"using System;class Test {private int member; }");
        }

        [Test]
        public void TestInspectorCase2()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {private int member;$public Test(){}$ static Test(){}}", @"using System;class Test {private int member;static Test(){}}");
        }

        [Test]
        public void TestResharperDisable()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;
#pragma warning disable " + CSharpDiagnosticIDs.EmptyConstructorAnalyzerID + @"
class Test {
	public Test(){
	}
	}");
        }

        [Test]
        public void TestNegateCase1()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {public Test(){Foo();}}");
        }

        [Test]
        public void TestNegateCase2()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {public Test(){Bar();} private Test(){}}");
        }

        [Test]
        public void TestNegateCase3()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {public Test() : base(4) {}}");
        }
    }
}