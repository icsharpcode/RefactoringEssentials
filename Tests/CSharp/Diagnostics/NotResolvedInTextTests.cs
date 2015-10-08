using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class NotResolvedInTextTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestBadExamples()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F()
	{
		throw new ArgumentNullException($""The parameter 'blah' can not be null""$, ""blah"");
		throw new ArgumentException(""blah"", $""The parameter 'blah' can not be null""$);
		throw new ArgumentOutOfRangeException($""The parameter 'blah' can not be null""$, ""blah"");
		throw new DuplicateWaitObjectException($""The parameter 'blah' can not be null""$, ""blah"");
	}
}");
        }

        [Test]
        public void TestArgumentNullExceptionSwap()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentNullException(""bar"", $""foo""$);
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentNullException(""foo"", ""bar"");
	}
}");
        }

        [Test]
        public void TestArgumentExceptionSwap()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentException($""foo""$, ""bar"");
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentException(""bar"", ""foo"");
	}
}", 0, 0);
        }

        [Test]
        public void TestArgumentOutOfRangeExceptionSwap()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException(""bar"", $""foo""$);
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException(""foo"", ""bar"");
	}
}", 0);
        }

        [Test]
        public void TestArgumentOutOfRangeExceptionSwapCase2()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException(""bar"", 3, $""foo""$);
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new ArgumentOutOfRangeException(""foo"", 3, ""bar"");
	}
}", 0, 0);
        }

        [Test]
        public void TestDuplicateWaitObjectExceptionSwap()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(int foo)
	{
		throw new DuplicateWaitObjectException(""bar"", $""foo""$);
	}
}", @"
using System;
class A
{
	void F(int foo)
	{
		throw new DuplicateWaitObjectException(""foo"", ""bar"");
	}
}", 0);
        }


        [Test]
        public void TestInvalidArgumentException()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"");
	}
}");
        }

        [Test]
        public void TestArgumentExceptionGuessing()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", $""bar""$);
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", ""foo"");
	}
}");
        }

        [Test]
        public void TestArgumentExceptionGuessingCase2()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", $""bar""$, new Exception());
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentException(""bar"", ""foo"", new Exception());
	}
}");
        }

        [Test]
        public void TestArgumentNullGuessing()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentNullException($""bar""$);
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentNullException(""foo"");
	}
}", 0, 1);
        }

        [Test]
        public void TestArgumentNullGuessingResolve2()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new System.ArgumentNullException($""bar""$);
	}
}", @"
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new System.ArgumentNullException(""foo"", ""bar"");
	}
}", 0, 0);
        }

        /// <summary>
        /// Source analysis can't resolve 'key' in indexer property setter 
        /// </summary>
        [Test]
        public void TestArgumentNullOnIndexerKey()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	public string this[string key] {
		get {}
		set {
			if(key == null) throw new ArgumentNullException(""key"");
            if(value == null) throw new ArgumentNullException(""value"");
		}
	}
}");
        }

        [Test]
        public void TestArgumentNullGuessingCase2()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentNullException($""bar""$, ""test"");
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentNullException(""foo"", ""test"");
	}
}");
        }

        [Test]
        public void TestArgumentOutOfRangeExceptionGuessing()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(int foo, int bar)
	{
		if(foo < 0 || foo > 10)
			throw new ArgumentOutOfRangeException($""foobar""$, ""foobar"");
	}
}", @"
using System;
class A
{
	void F(int foo, int bar)
	{
		if(foo < 0 || foo > 10)
			throw new ArgumentOutOfRangeException(""foo"", ""foobar"");
	}
}");
        }

        [Test]
        public void TestArgumentOutOfRangeExceptionGuessingCase2()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentOutOfRangeException($""bar""$, null, ""bar"");
	}
}", @"
using System;
class A
{
	void F(object foo)
	{
		if(foo != null)
			throw new ArgumentOutOfRangeException(""foo"", null, ""bar"");
	}
}");
        }

        [Test]
        public void TestConstructorValidCase()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	public A(BaseRefactoringContext context, Statement rootStatement, IEnumerable<ParameterDeclaration> parameters, CancellationToken cancellationToken)
	{
		if(rootStatement == null)
			throw new ArgumentNullException(""rootStatement"");
		if(context == null)
			throw new ArgumentNullException(""context"");
	}
}");
        }

        /// <summary>
        /// Bug 15039 - source analysis can't resolve 'value' in property setter 
        /// </summary>
        [Test]
        public void TestBug15039()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	public string Foo {
		get {}
		set {
			if(value == null)
				throw new ArgumentNullException(""value"");
		}
	}
}");
        }

        [Test]
        public void TestValue()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
	public string Foo {
		get {}
		set {
			if(value == null)
				throw new ArgumentNullException($""val""$);
		}
	}
}", @"
using System;
class A
{
	public string Foo {
		get {}
		set {
			if(value == null)
				throw new ArgumentNullException(""value"");
		}
	}
}", 0, 1);
        }

        [Test]
        public void TestIssue45()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
    public string this[string key]
    {
       set
       {
           if (key == null) throw new ArgumentNullException(""key"");
       }
    }
}");
        }

        [Test]
        public void TestIssue120_ConversionOperator()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
    public static implicit operator A(string key)
    {
       set
       {
           if (key == null) throw new ArgumentNullException(""key"");
       }
    }
}");
        }

        [Test]
        public void TestOperator()
        {
            Analyze<NotResolvedInTextAnalyzer>(@"
using System;
class A
{
    public static implicit operator true(A x)
    {
       set
       {
           if (x == null) throw new ArgumentNullException(""x"");
       }
    }
}");
        }
    }
}
