using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class PossibleAssignmentToReadonlyFieldTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestTypeParameter()
        {
            TestIssue<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
interface IFoo
{
	int Property { get; set; }
}

class FooBar<T> where T : IFoo
{
	readonly T field;

	void Test ()
	{
		field.Property = 7;
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestClassTypeParameter()
        {
            Analyze<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
interface IFoo
{
	int Property { get; set; }
}

class FooBar<T> where T : class, IFoo
{
	readonly T field;

	void Test ()
	{
		field.Property = 7;
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestValueType()
        {
            TestIssue<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
struct Bar
{
	public int P;
}

class FooBar
{
	readonly Bar field;

	public static void Foo()
	{
		var a = new FooBar();
		a.field.P = 7;
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestConstructor()
        {
            Analyze<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
interface IFoo
{
	int Property { get; set; }
}

class FooBar<T> where T : IFoo
{
	readonly T field;

	public FooBar (T t)
	{
		this.field = t;
		this.field.Property = 5;
	}
}
");
        }

        /// <summary>
        /// Bug 15038 - readonly member property is incorrectly underlined in the .ctor when initialized 
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug15038()
        {
            Analyze<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
using System;
using System.Collections.Generic;

public class Multipart
{
	readonly List<MimeEntity> children;
	
	internal Multipart ()
	{
		children = new List<MimeEntity> ();
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
interface IFoo
{
	int Property { get; set; }
}

struct Bar : IFoo
{
	public int Property { get; set; }
}

class FooBar<T> where T : IFoo
{
	readonly T field;

	public static void Foo()
	{
		var a = new FooBar<Bar>();
		// ReSharper disable once PossibleAssignmentToReadonlyField
		a.field.Property = 7;
	}
}
");
        }

        /// <summary>
        /// Bug 15109 - Incorrect "Readonly field cannot be used as assignment target" error
        /// </summary>
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug15109()
        {
            Analyze<PossibleAssignmentToReadonlyFieldAnalyzer>(@"
namespace TestProject 
{
	class FileInfo {
		public int Foo { get; set; } 
	}
	class Program
	{
		readonly FileInfo f = new FileInfo ();

		void Test ()
		{
			f.Foo = 12;
		}
	}
}
");
        }
    }

}

