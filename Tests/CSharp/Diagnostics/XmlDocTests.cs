using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class XmlDocTests : InspectionActionTestBase
    {
        [Test]
        public void TestBeforeNamespace()
        {
            Test<XmlDocAnalyzer>(@"
/// foo
namespace Foo {}
", @"
namespace Foo {}
");
        }

        [Test]
        public void TestBeforeUsing()
        {
            Test<XmlDocAnalyzer>(@"
/// foo
using System;
", @"
using System;
");
        }

        [Test]
        public void TestBeforeUsingAlias()
        {
            Test<XmlDocAnalyzer>(@"
/// foo
using A = System;
", @"
using A = System;
");
        }

        [Test]
        public void TestBeforeExternAlias()
        {
            Test<XmlDocAnalyzer>(@"
/// foo
extern alias System;
", @"
extern alias System;
");
        }

        [Test]
        public void TestTypeParameter()
        {
            TestIssue<XmlDocAnalyzer>(@"
/// <typeparam name=""Undefined""></typeparam>
class Foo {}

/// <typeparam name=""T""></typeparam>
class Foo2<T> {}
");
        }

        [Test]
        public void TestWrongMethodParameter()
        {
            TestIssue<XmlDocAnalyzer>(@"
class Foo {
	/// <param name=""undefined""></param>
	/// <param name=""y""></param>
	/// <param name=""z""></param>
	public void FooBar(int x, int y, int z)
	{
	}

	/// <param name=""x1""></param>
	/// <param name=""y""></param>
	int this[int x, int y] { get { return 1;  } }
}
", 2);
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
	}
}
");
        }

        [Test]
        public void TestEventComment()
        {
            TestIssue<XmlDocAnalyzer>(@"
using System;

namespace Foo
{
	public interface IGroupingProvider
	{
		/// <summa
		event EventHandler<EventArgs> NextChanged;
	}
}
", 1);
        }
    }


}

