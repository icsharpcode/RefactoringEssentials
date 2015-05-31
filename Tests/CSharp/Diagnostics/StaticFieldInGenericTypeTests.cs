using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{

    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class StaticFieldInGenericTypeTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void GenericClass()
        {
            TestIssue<StaticFieldInGenericTypeAnalyzer>(@"
class Foo<T>
{
	static string Data;
}");
        }

        [Test]
        public void GenericClassWithGenericField()
        {
            Analyze<StaticFieldInGenericTypeAnalyzer>(@"
class Foo<T>
{
	static System.Collections.Generic.IList<T> Cache;
}");
        }

        [Test]
        public void GenericClassWithMultipleGenericFields()
        {
            TestIssue<StaticFieldInGenericTypeAnalyzer>(@"
class Foo<T1, T2>
{
	static System.Collections.Generic.IList<T1> Cache;
}");
        }

        [Test]
        public void NestedGenericClassWithGenericField()
        {
            TestIssue<StaticFieldInGenericTypeAnalyzer>(@"
class Foo<T1>
{
	class Bar<T2>
	{
		static System.Collections.Generic.IList<T1> Cache;
	}
}");
        }

        [Test]
        public void NonGenericClass()
        {
            Analyze<StaticFieldInGenericTypeAnalyzer>(@"
class Foo
{
	static string Data;
}");
        }

        [Test]
        public void NonStaticField()
        {
            Analyze<StaticFieldInGenericTypeAnalyzer>(@"
class Foo<T>
{
	string Data;
}");
        }

        [Test]
        public void TestMicrosoftSuppressMessage()
        {
            TestIssue<StaticFieldInGenericTypeAnalyzer>(@"using System.Diagnostics.CodeAnalysis;

class Foo<T>
{
	[SuppressMessage(""Microsoft.Design"", ""CA1000:DoNotDeclareStaticMembersOnGenericTypes"")]
	static string Data;

	static string OtherData;
}");
        }

        [Test]
        public void TestAssemblyMicrosoftSuppressMessage()
        {
            Analyze<StaticFieldInGenericTypeAnalyzer>(@"using System.Diagnostics.CodeAnalysis;

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
    // ReSharper disable once StaticFieldInGenericType
	static string Data;
}";
            Analyze<StaticFieldInGenericTypeAnalyzer>(input);
        }

    }
}

