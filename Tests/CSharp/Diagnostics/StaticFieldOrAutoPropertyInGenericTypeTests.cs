using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StaticFieldOrAutoPropertyInGenericTypeTests : CSharpDiagnosticTestBase
    {

        [Fact]
        public void GenericClass()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
	static string $Data$;
}");
        }


        [Fact]
        public void AutoProperty()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
    static string $Data$ { get ; set; };
}");
        }

        [Fact]
        public void GenericClassWithGenericField()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
	static System.Collections.Generic.IList<T> Cache;
}");
        }

        [Fact]
        public void GenericClassWithMultipleGenericFields()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T1, T2>
{
	static System.Collections.Generic.IList<T1> $Cache$;
}");
        }

        [Fact]
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

        [Fact]
        public void NonGenericClass()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo
{
	static string Data;
}");
        }

        [Fact]
        public void NonStaticField()
        {
            Analyze<StaticFieldOrAutoPropertyInGenericTypeAnalyzer>(@"
class Foo<T>
{
	string Data;
}");
        }

        [Fact(Skip="Not yet supported")]
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

        [Fact(Skip="Not yet supported")]
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

        [Fact]
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

