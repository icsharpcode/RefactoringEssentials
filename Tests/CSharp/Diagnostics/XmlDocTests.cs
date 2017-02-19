using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class XmlDocTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestBeforeNamespace()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$namespace Foo {}
");
        }

        [Fact]
        public void TestBeforeUsing()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$using System;
");
        }

        [Fact]
        public void TestBeforeUsingAlias()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$using A = System;
");
        }

        [Fact]
        public void TestBeforeExternAlias()
        {
            Analyze<XmlDocAnalyzer>(@"
$/// foo
$extern alias System;
");
        }

        [Fact]
        public void TestTypeParameter()
        {
            Analyze<XmlDocAnalyzer>(@"
/// <typeparam name=""$Undefined$""></typeparam>
class Foo {}

/// <typeparam name=""T""></typeparam>
class Foo2<T> {}
");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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
        [Fact]
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


        [Fact]
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

        [Fact]
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
        /// <summary>
        /// XmlDocAnalyzer causes unexpected exception which crashes Visual Studio #180
        /// </summary>
        [Fact]
        public void TestIssue180()
        {
            Analyze<XmlDocAnalyzer>(@"
class Foo {
    /// <param name=""$>$</param>
    public void FooBar(int x, int y, int z)
    {
    }
}
");
        }

        [Fact]
        public void TestDelegateDeclaration()
        {
            Analyze<XmlDocAnalyzer>(@"
class Foo {
    /// <summary>
    /// </summary>
    /// <param name=""$message$"">The data.</param>
    public delegate void FooEventHandler(byte[] data);

    /// <summary>
    /// </summary>
    /// <param name=""data"">The data.</param>
    public delegate void BarEventHandler(byte[] data);
}
");
        }
    }
}

