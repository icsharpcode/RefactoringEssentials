using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class MissingInterfaceMemberImplementationTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnimplementedInterface()
        {
            TestIssue<MissingInterfaceMemberImplementationAnalyzer>(@"
interface IF
{
	void Foo();
}

class Test : IF
{
}
");
        }

        [Test]
        public void TestPartiallyImplementedInterface()
        {
            TestIssue<MissingInterfaceMemberImplementationAnalyzer>(@"
interface IF
{
	void Foo();
	void Foo2();
}

class Test : IF
{
	public void Foo()
	{
	}
}
");
        }

        [Test]
        public void TestMultiple()
        {
            TestIssue<MissingInterfaceMemberImplementationAnalyzer>(@"
interface IF
{
	void Foo();
}

interface IB
{
	void Bar();
	void Bar2();
}


class Test : IF, IB
{
	public void Bar()
	{
	}
}
", 2);
        }

        [Test]
        public void TestImplementedInterface()
        {
            Analyze<MissingInterfaceMemberImplementationAnalyzer>(@"
interface IF
{
	void Foo();
}

class Test : IF
{
	public void Foo()
	{
	}
}
");
        }


        [Test]
        public void TestInterfaceInheritance()
        {
            Analyze<MissingInterfaceMemberImplementationAnalyzer>(@"
	public interface IService
	{
		string ServiceName { get; }
	}

	public interface IExtensionService : IService
	{
		void Initialize ();
	}
");
        }


        /// <summary>
        /// Bug 14944 - Incorrect issue context for Interface not implemented 
        /// </summary>
        [Test]
        public void TestBug14944()
        {
            Analyze<MissingInterfaceMemberImplementationAnalyzer>(@"
using System.Collections;
using System.Collections.Generic;
using System;

abstract class Test<T> : IEnumerable, IEnumerable<T>
{
	#region IEnumerable implementation

	public IEnumerator<T> GetEnumerator ()
	{
		throw new NotImplementedException ();
	}

	#endregion

	#region IEnumerable implementation

	IEnumerator IEnumerable.GetEnumerator ()
	{
		throw new NotImplementedException ();
	}
	#endregion
}
");
        }


        [Test]
        public void TestAlreadyImplementedByInheritance()
        {
            Analyze<MissingInterfaceMemberImplementationAnalyzer>(@"
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class FooBase<T> : IEnumerable, IEnumerable<T>
{
	public IEnumerator<T> GetEnumerator ()
	{
		return null;
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return null;
	}
}

public class Foo<T> : FooBase<T>, IEnumerable, IEnumerable<T>
{
}
");
        }


    }
}

