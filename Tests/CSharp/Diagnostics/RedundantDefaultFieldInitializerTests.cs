using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantDefaultFieldInitializerTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void TestRedundantIntInitializer()
        {
            var input = @"
class TestClass
{
	int i = 0;
	long l = 0L;
}";
            var output = @"
class TestClass
{
	int i;
	long l;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 2, output);
        }

        [Test]
        public void TestRedundantFloatInitializer()
        {
            var input = @"
class TestClass
{
	double d = 0;
	double d2 = 0.0;
}";
            var output = @"
class TestClass
{
	double d;
	double d2;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 2, output);
        }

        [Test]
        public void TestRedundantBooleanInitializer()
        {
            var input = @"
class TestClass
{
	bool x = false;
}";
            var output = @"
class TestClass
{
	bool x;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestRedundantCharInitializer()
        {
            var input = @"
class TestClass
{
	char ch = '\0';
}";
            var output = @"
class TestClass
{
	char ch;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestRedundantReferenceTypeInitializer()
        {
            var input = @"
class TestClass
{
	string str = null;
}";
            var output = @"
class TestClass
{
	string str;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestRedundantDynamicInitializer()
        {
            var input = @"
class TestClass
{
	dynamic x = null, y = null;
}";
            var output = @"
class TestClass
{
	dynamic x, y;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 2, output);
        }

        [Test]
        public void TestRedundantStructInitializer()
        {
            var input = @"
struct TestStruct
{
}
class TestClass
{
	TestStruct x = new TestStruct ();
}";
            var output = @"
struct TestStruct
{
}
class TestClass
{
	TestStruct x;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 1, output);
        }

        [Test]
        public void TestRedundantNullableInitializer()
        {
            var input = @"
class TestClass
{
	int? i = null;
}";
            var output = @"
class TestClass
{
	int? i;
}";
            Test<RedundantDefaultFieldInitializerAnalyzer>(input, 1, output);
        }


        [Test]
        public void TestRedundantConstantBug()
        {
            Test<RedundantDefaultFieldInitializerAnalyzer>(@"class Test { const int foo = 0;  }", 0);
        }

        [Test]
        public void TestRedundantReadOnlyBug()
        {
            Test<RedundantDefaultFieldInitializerAnalyzer>(@"struct Test { static readonly Test foo = new Test ();  }", 0);
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
    // ReSharper disable once RedundantDefaultFieldInitializer
	int i = 0;
}";
            Analyze<RedundantDefaultFieldInitializerAnalyzer>(input);
        }




    }
}
