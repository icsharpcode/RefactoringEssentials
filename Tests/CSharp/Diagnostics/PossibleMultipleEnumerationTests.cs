using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class PossibleMultipleEnumerationTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVariableInvocation()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		var type = e.GetType();
		var x = e.First ();
		var y = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(
                input,
                2,
                @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		var type = e.GetType();
		var enumerable = e as object[] ?? e.ToArray ();
		var x = enumerable.First ();
		var y = enumerable.Count ();
	}
}", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVariableForeach()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		foreach (var x in e) ;
		foreach (var y in e) ;
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(
                input,
                2,
                @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		var enumerable = e as IList<object> ?? e.ToList ();
		foreach (var x in enumerable) ;
		foreach (var y in enumerable) ;
	}
}", 1, 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestVariableMixed()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		foreach (var x in e) ;
		var y = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestParameter()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (IEnumerable<object> e)
	{
		foreach (var x in e) ;
		var y = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestObjectMethodInvocation()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e;
		var a = e.GetType ();
		var b = e.ToString ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIf()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		if (i > 0) {
			var a = e.Count ();
		} else {
			var b = e.First ();
			var c = e.Count ();
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIf2()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		if (i > 0) {
			var a = e.Count ();
		} else {
			var b = e.First ();
		}
		var c = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 3);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestIf3()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		if (i > 0) {
			var a = e.Count ();
		} else {
			var b = e.First ();
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFor()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e;
		for (int i = 0; i < 10; i++) {
			var a = e.Count ();
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestWhile()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e;
		int i;
		while (i > 1) {
			var a = e.Count ();
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestWhile2()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e;
		int i;
		while (i > e.Count ()) {
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 1);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestWhile3()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e;
		int i;
		object x;
		while (true) {
			if (i > 1) {
				x = e.First ();
				break;
			} 
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestWhile4()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	IEnumerable<object> GetEnum () { }
	void TestMethod (int i)
	{
		IEnumerable<object> e = GetEnum ();
		var a1 = e.First ();
		while ((e = GetEnum ()) != null) {
			var a2 = e.First ();
		}
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDo()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	IEnumerable<object> GetEnum () { }
	void TestMethod (int i)
	{
		IEnumerable<object> e = GetEnum ();
		var a1 = e.First ();
		do {
			var a2 = e.First ();
		} while ((e = GetEnum ()) != null);
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDo2()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	IEnumerable<object> GetEnum () { }
	void TestMethod (int i)
	{
		IEnumerable<object> e = GetEnum ();
		do {
			var a2 = e.First ();
		} while ((e = GetEnum ()) != null);
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLambda()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		IEnumerable<object> e;
		Action a = () => {
			var x = e.Count ();
			var y = e.Count ();
		};
		var z = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLambda2()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void Test (object a, object b) { }
	void TestMethod ()
	{
		IEnumerable<object> e;
		Action a = () => Test(e.First (), e.Count ());
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLambda3()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void Test (object a, Action b) { }
	void TestMethod ()
	{
		IEnumerable<object> e;
		Test(e.First (), () => e.Count ());
		e = null;
		var x = e.First ();
		Action a = () => e.Count();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLambda4()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void Test (object a, object b) { }
	void TestMethod ()
	{
		IEnumerable<object> e;
		Action a = () => Test(e.ToString (), e.ToString ());
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestConditionalExpression()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		var a = i > 0 ? e.First () : e.FirstOrDefault ();
		Action b = () => i > 0 ? e.First () : e.FirstOrDefault ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestConditionalExpression2()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		var a = i > 0 ? e.First () : new object ();
		var b = e.First ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestConstantConditionalExpression()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		var a = 1 > 2 ? e.First () : new object ();
		var b = e.First ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInConditionalExpression()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	IEnumerable<object> GetEnum () { }
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		var x1 = e.First ();
		var a = i > 0 ? e = GetEnum () : GetEnum ();
		var x2 = e.First ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignmentInConditionalExpression2()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	IEnumerable<object> GetEnum () { }
	void TestMethod (int i)
	{
		IEnumerable<object> e;
		var x1 = e.First ();
		var a = i > 0 ? e = GetEnum () : e = GetEnum ();
		var x2 = e.First ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignment()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (IEnumerable<object> e)
	{
		foreach (var x in e) ;
		e = null;
		var y = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAssignment2()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (IEnumerable<object> e)
	{
		foreach (var x in e) ;
		e = null;
		var y = e.Count ();
		e = null;
		var a = e.First ();
		var b = e.First ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNoIssue()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod (IEnumerable<object> e)
	{
		foreach (var x in e) ;
		IEnumerable<object> e2;
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestExpression()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	int Test (params object[] args) { }
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		var type = e.GetType();
		var x = Test (e.First (), e.Count ());
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestExpression2()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	int Test (params object[] args) { }
	void TestMethod ()
	{
		IEnumerable<object> e = null;
		var type = e.GetType();
		var x = Test (e.First (), e = new objct[0], e.Count ());
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOutArgument()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void Test (out IEnumerable<object> e)
	{
		e = null;
	}

	void TestMethod (IEnumerable<object> e)
	{
		foreach (var x in e) ;
		Test (out e);
		var y = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOutArgument2()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void Test (out IEnumerable<object> e)
	{
		e = null;
	}

	void TestMethod (IEnumerable<object> e)
	{
		foreach (var x in e) ;
		Test (out e);
		var y = e.Count ();
		var z = e.Count ();
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOutArgument3()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void Test (object arg1, out IEnumerable<object> e, object arg2)
	{
		e = null;
	}

	void TestMethod (IEnumerable<object> e)
	{
		Test (e.First (), out e, e.First ());
	}
}";
            Test<PossibleMultipleEnumerationAnalyzer>(input, 2);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            var input = @"
using System.Collections.Generic;
using System.Linq;
class TestClass
{
	void TestMethod ()
	{
		// ReSharper disable PossibleMultipleEnumeration
		IEnumerable<object> e = null;
		var type = e.GetType();
		var x = e.First ();
		var y = e.Count ();
		// ReSharper restore PossibleMultipleEnumeration
	}
}";
            Analyze<PossibleMultipleEnumerationAnalyzer>(input);
        }
    }
}
