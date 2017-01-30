using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ThreadStaticAtInstanceFieldTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"using System;
class Foo
{
	[$ThreadStatic$]
	int bar;
}", @"using System;
class Foo
{
	int bar;
}");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"using System;
class Foo
{
	[Serializable, $ThreadStatic$]
	int bar;
}", @"using System;
class Foo
{
	[Serializable]
	int bar;
}");
        }

        [Fact]
        public void TestInspectorCase3()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"class Foo
{
	[$System.ThreadStatic$, System.Serializable]
	int bar;
}", @"class Foo
{
	[System.Serializable]
	int bar;
}");
        }



        [Fact]
        public void TestDisable()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"using System;
class Foo
{
#pragma warning disable " + CSharpDiagnosticIDs.ThreadStaticAtInstanceFieldAnalyzerID + @"

	[ThreadStatic]
	int bar;
}");

        }


        [Fact]
        public void InstanceField()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"
using System;
class TestClass
{
	[$ThreadStatic$]
	string field;
}", @"
using System;
class TestClass
{
	string field;
}");
        }

        [Fact]
        public void InstanceFieldWithMultiAttributeSection()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"
using System;
class TestClass
{
	[field: $ThreadStatic$, ContextStatic]
	string field;
}", @"
using System;
class TestClass
{
	[field: ContextStatic]
	string field;
}");
        }

        [Fact]
        public void StaticField()
        {
            Analyze<ThreadStaticAtInstanceFieldAnalyzer>(@"
using System;
class TestClass
{
	[ThreadStatic]
	static string field;
}");
        }
    }
}

