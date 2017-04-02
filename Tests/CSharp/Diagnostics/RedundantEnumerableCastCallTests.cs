using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantEnumerableCastCallTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRedundantCast()
        {
            Test<RedundantEnumerableCastCallAnalyzer>(@"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args.Cast<string> ();
	}
}
", @"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args;
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRedundantCastCase2()
        {
            Test<RedundantEnumerableCastCallAnalyzer>(@"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (string[] args)
	{
		var a = args.Cast<string> ();
	}
}
", @"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (string[] args)
	{
		var a = args;
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRedundantOfType()
        {
            Test<RedundantEnumerableCastCallAnalyzer>(@"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args.OfType<string> ();
	}
}
", @"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args.Where (i => i != null);
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestRedundantOfTypeResolution2()
        {
            Test<RedundantEnumerableCastCallAnalyzer>(@"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args.OfType<string> ();
	}
}
", @"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args;
	}
}
", 1);
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalid()
        {
            Analyze<RedundantEnumerableCastCallAnalyzer>(@"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		var a = args.Cast<object> ();
	}
}
");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDisable()
        {
            Analyze<RedundantEnumerableCastCallAnalyzer>(@"
using System;
using System.Linq;
using System.Collections.Generic;

class Test
{
	static void Main (IEnumerable<string> args)
	{
		// ReSharper disable once RedundantEnumerableCastCall
		var a = args.Cast<string> ();
	}
}
");
        }

    }
}

