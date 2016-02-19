using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantDefaultFieldInitializerTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void TestRedundantIntInitializer()
        {
            var input = @"
class TestClass
{
	int i $= 0$;
	long l $= 0L$;
}";
            var output = @"
class TestClass
{
	int i;
	long l;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestRedundantFloatInitializer()
        {
            var input = @"
class TestClass
{
	double d $= 0$;
	double d2 $= 0.0$;
}";
            var output = @"
class TestClass
{
	double d;
	double d2;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestRedundantBooleanInitializer()
        {
            var input = @"
class TestClass
{
	bool x $= false$;
}";
            var output = @"
class TestClass
{
	bool x;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestRedundantCharInitializer()
        {
            var input = @"
class TestClass
{
	char ch $= '\0'$;
}";
            var output = @"
class TestClass
{
	char ch;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestRedundantReferenceTypeInitializer()
        {
            var input = @"
class TestClass
{
	string str $= null$;
}";
            var output = @"
class TestClass
{
	string str;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestRedundantDynamicInitializer()
        {
            var input = @"
class TestClass
{
	dynamic x $= null$, y $= null$;
}";
            var output = @"
class TestClass
{
	dynamic x, y;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
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
	TestStruct x $= default(TestStruct)$;
}";
            var output = @"
struct TestStruct
{
}
class TestClass
{
	TestStruct x;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }

        [Test]
        public void TestRedundantNullableInitializer()
        {
            var input = @"
class TestClass
{
	int? i $= null$;
}";
            var output = @"
class TestClass
{
	int? i;
}";
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(input, output);
        }


        [Test]
        public void TestRedundantConstantBug()
        {
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(@"class Test { const int foo = 0;  }");
        }

        [Test]
        public void TestRedundantReadOnlyBug()
        {
                Analyze<RedundantDefaultFieldInitializerAnalyzer>(@"struct Test { static readonly Test foo = new Test ();  }");
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
#pragma warning disable " + CSharpDiagnosticIDs.RedundantDefaultFieldInitializerAnalyzerID + @"
	int i = 0;
}";
            Analyze<RedundantDefaultFieldInitializerAnalyzer>(input);
        }
    }
}