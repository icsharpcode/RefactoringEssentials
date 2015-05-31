using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class PossibleAssignmentToReadonlyFieldTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
        [Test]
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

        [Test]
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
        [Test]
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

