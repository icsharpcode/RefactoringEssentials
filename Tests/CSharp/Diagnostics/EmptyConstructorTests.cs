using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EmptyConstructorTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {private int member; $public Test(){}$}", @"using System;class Test {private int member; }");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {private int member;$public Test(){}$ static Test(){}}", @"using System;class Test {private int member;static Test(){}}");
        }

        [Fact]
        public void TestResharperDisable()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;
#pragma warning disable " + CSharpDiagnosticIDs.EmptyConstructorAnalyzerID + @"
class Test {
	public Test(){
	}
	}");
        }

        [Fact]
        public void TestNegateCase1()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {public Test(){Foo();}}");
        }

        [Fact]
        public void TestNegateCase2()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {public Test(){Bar();} private Test(){}}");
        }

        [Fact]
        public void TestNegateCase3()
        {
            Analyze<EmptyConstructorAnalyzer>(@"using System;class Test {public Test() : base(4) {}}");
        }
    }
}