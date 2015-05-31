using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantEnumerableCastCallTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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


        [Test]
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


        [Test]
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

