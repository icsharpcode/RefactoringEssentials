using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class XmlDocTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBeforeNamespace()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$namespace Foo {}
");
        }

        [Test]
        public void TestBeforeUsing()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$using System;
");
        }

        [Test]
        public void TestBeforeUsingAlias()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$using A = System;
");
        }

        [Test]
        public void TestBeforeExternAlias()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$extern alias System;
");
        }

        [Test]
        public void TestTypeParameter()
        {
            Analyze<XmlDocAnalyzer>(@"
/// <typeparam name=""$Undefined$""></typeparam>
class Foo {}

/// <typeparam name=""T""></typeparam>
class Foo2<T> {}
");
        }

        [Test]
        public void TestWrongMethodParameter()
        {
            Analyze<XmlDocAnalyzer>(@"
class Foo {
	/// <param name=""$undefined$""></param>
	/// <param name=""y""></param>
	/// <param name=""z""></param>
	public void FooBar(int x, int y, int z)
	{
	}

	/// <param name=""$x1$""></param>
	/// <param name=""y""></param>
	int this[int x, int y] { get { return 1;  } }
}
");
        }

        [Test]
        public void TestSeeCref()
        {
            Analyze<XmlDocAnalyzer>(@"
/// <summary>
/// </summary>
/// <see cref=""Undefined""/>
class Foo {
	public void Undefined () {}
}

/// <summary>
/// <seealso cref=""Foo""/>
/// </summary>
/// <see cref=""Foo2""/>
class Foo2 {}
");
        }

        [Test]
        public void TestValidCref()
        {
            Analyze<XmlDocAnalyzer>(@"
using System;

namespace Foo {
	/// <summary>
	/// </summary>
	/// <see cref=""IDisposable""/>
	class Foo {
		public void Undefined () {}
	}
}");
        }

        /// <summary>
        /// Bug 17729 - Incorrect XML-docs warning about 'value' paramref 
        /// </summary>
        [Test]
        public void TestBug17729()
        {
            Analyze<XmlDocAnalyzer>(@"
using System;

class Foo {
	/// <summary>
	/// If <paramref name=""value""/> is 0 ...
	/// </summary>
	public int Bar { get; set; }
}
");
        }


        [Test]
        public void TestSeeCRefMember()
        {
            Analyze<XmlDocAnalyzer>(@"
using System;

namespace Foo
{
	public interface IGroupingProvider
	{
		IGroupingProvider Next { get; set; }
		
        /// <summary>
        /// Occurs when <see cref=""Next""/> changes.
        /// </summary>
        event EventHandler<EventArgs> NextChanged;

        /// <summary>
        /// Occurs when <see cref=""$Next2$""/> changes.
        /// </summary>
        event EventHandler<EventArgs> OtherNextChanged;
	}
}
");
        }

        [Test]
        public void TestEventComment()
        {
            Analyze<XmlDocAnalyzer>(@"
using System;

namespace Foo
{
	public interface IGroupingProvider
	{
		/// <summa
$$		event EventHandler<EventArgs> NextChanged;
	}
}
");
        }
    }


}

