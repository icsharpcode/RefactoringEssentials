using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture, Ignore("Not implemented!")]
    public class IterateViaForeachTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void HandlesNonGenericCase()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections;
class TestClass
{
	public void F()
	{
		var $list = new ArrayList ();
	}
}", @"
using System.Collections;
class TestClass
{
	public void F()
	{
		var list = new ArrayList ();
		foreach (var o in list) {
		}
	}
}");
        }

        [Test]
        public void HandlesExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	public IEnumerable<int> GetInts()
	{
		return new int[] { };
	}

	public void F()
	{
		GetInts$();
	}
}", @"
using System.Collections.Generic;
class TestClass
{
	public IEnumerable<int> GetInts()
	{
		return new int[] { };
	}

	public void F()
	{
		foreach (var i in GetInts ()) {
		}
	}
}");
        }

        [Test]
        public void HandlesAssignmentExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	public IEnumerable<int> GetInts ()
	{
		return new int[] { };
	}

	public void F()
	{
		IEnumerable<int> ints;
		$ints = GetInts ();
	}
}", @"
using System.Collections.Generic;
class TestClass
{
	public IEnumerable<int> GetInts ()
	{
		return new int[] { };
	}

	public void F()
	{
		IEnumerable<int> ints;
		ints = GetInts ();
		foreach (var i in ints) {
		}
	}
}");
        }

        [Test]
        public void HandlesAsExpressionStatement()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	public void F()
	{
		object s = """";
		s as IEnumerable$<char>;
	}
}", @"
using System.Collections.Generic;
class TestClass
{
	public void F()
	{
		object s = """";
		foreach (var c in s as IEnumerable<char>) {
		}
	}
}", 0, false);
        }

        [Test]
        public void NonKnownTypeNamingTest()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	public void F()
	{
		var $items = new List<TestClass> ();
	}
}", @"
using System.Collections.Generic;
class TestClass
{
	public void F()
	{
		var items = new List<TestClass> ();
		foreach (var testClass in items) {
		}
	}
}");
        }

        [Test]
        public void HandlesAsExpression()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	public void F()
	{
		object s = """";
		s as IEnumerable$<char>
	}
}", @"
using System.Collections.Generic;
class TestClass
{
	public void F()
	{
		object s = """";
		foreach (var c in s as IEnumerable<char>) {
		}
	}
}", 0, true);
        }

        [Test]
        public void HandlesLinqExpressionAssignment()
        {
            Test<IterateViaForeachAction>(@"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	public IEnumerable<int> GetInts()
	{
		return new int[] { };
	}

	public void F()
	{
		var $filteredInts = from item in GetInts ()
							where item > 0
							select item;
	}
}", @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	public IEnumerable<int> GetInts()
	{
		return new int[] { };
	}

	public void F()
	{
		var filteredInts = from item in GetInts ()
							where item > 0
							select item;
		foreach (var i in filteredInts) {
		}
	}
}");
        }

        [Test]
        public void IgnoresExpressionsInForeachStatement()
        {
            TestWrongContext<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	public IEnumerable<int> GetInts()
	{
		return new int[] { };
	}

	public void F()
	{
		foreach (var i in $GetInts ()) {
		}
	}
}");
        }

        [Test]
        public void IgnoresInitializersInForStatement()
        {
            TestWrongContext<IterateViaForeachAction>(@"
class TestClass
{
	public void F()
	{
		for (int[] i = new $int[] {} ;;) {
		}
	}
}");
        }

        [Test]
        public void AddsForToBodyOfUsingStatement()
        {
            Test<IterateViaForeachAction>(@"
class TestClass
{
	public void F()
	{
		using (int[] $i = new int[] {}) {
		}
	}
}", @"
class TestClass
{
	public void F()
	{
		using (int[] i = new int[] {}) {
			foreach (var j in i) {
			}
		}
	}
}");
        }

        [Test]
        public void AddsBlockStatementToUsingStatement()
        {
            Test<IterateViaForeachAction>(@"
class TestClass
{
	public void F()
	{
		using (int[] $i = new int[] {});
	}
}", @"
class TestClass
{
	public void F()
	{
		using (int[] i = new int[] { }) {
			foreach (var j in i) {
			}
		}
	}
}");
        }

        [Test]
        public void IgnoresFieldDeclarations()
        {
            TestWrongContext<IterateViaForeachAction>(@"
using System.Collections.Generic;
class TestClass
{
	List<int> list = $new List<int>();
}");
        }
    }
}

