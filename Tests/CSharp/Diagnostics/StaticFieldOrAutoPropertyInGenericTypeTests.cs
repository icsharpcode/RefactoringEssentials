using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class StaticFieldOrAutoPropertyInGenericTypeTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void GenericClass()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
	static string $Data$;
}");
        }


        [Test]
        public void AutoProperty()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
    static string $Data$ { get ; set; };
}");
        }

        [Test]
        public void GenericClassWithGenericField()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
	static System.Collections.Generic.IList<T> Cache;
}");
        }

        [Test]
        public void GenericClassWithMultipleGenericFields()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T1, T2>
{
	static System.Collections.Generic.IList<T1> $Cache$;
}");
        }

        [Test]
        public void NestedGenericClassWithGenericField()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T1>
{
	class Bar<T2>
	{
		static System.Collections.Generic.IList<T1> $Cache$;
	}
}");
        }

        [Test]
        public void NonGenericClass()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo
{
	static string Data;
}");
        }

        [Test]
        public void NonStaticField()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
	string Data;
}");
        }

        [Ignore("Not yet supported")]
        [Test]
        public void TestMicrosoftSuppressMessage()
        {
            TestIssue<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"using System.Diagnostics.CodeAnalysis;

class Foo<T>
{
	[SuppressMessage(""Microsoft.Design"", ""CA1000:DoNotDeclareStaticMembersOnGenericTypes"")]
	static string Data;

	static string OtherData;
}");
        }

        [Ignore("Not yet supported")]
        [Test]
        public void TestAssemblyMicrosoftSuppressMessage()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"using System.Diagnostics.CodeAnalysis;

[assembly:SuppressMessage(""Microsoft.Design"", ""CA1000:DoNotDeclareStaticMembersOnGenericTypes"")]

class Foo<T>
{
	static string Data;

	static string OtherData;
}");
        }

        [Test]
        public void TestDisable()
        {
            var input = @"using System.Diagnostics.CodeAnalysis;

class Foo<T>
{
#pragma warning disable " + CSharpDiagnosticIDs.StaticFieldInGenericTypeAnalyzerID + @"
	static string Data;
}";
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(input);
        }

    }
}

