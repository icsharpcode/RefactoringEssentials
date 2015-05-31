using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantToStringCallTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void ConcatenationOperator()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = """" + i.ToString() + """" + i.ToString();
	}
}", 2, @"
class Foo
{
	void Bar (object i)
	{
		string s = """" + i + """" + i;
	}
}");
        }

        [Test]
        public void TestValueTypes()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = """" + i.ToString() + """" + i.ToString();
	}
}");
        }


        [Test]
        public void ConcatenationOperatorWithToStringAsOnlyString()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = i.ToString() + i + i + i + 1.3;
	}
}");
        }

        [Test]
        public void IgnoresCallsToIFormattableToString()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (System.DateTime dt)
	{
		string s = dt.ToString("""", CultureInfo.InvariantCulture) + string.Empty;
	}
}");
        }

        [Test]
        public void StringTarget()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (string str)
	{
		string s = str.ToString();
		string inOperator = """" + str.ToString();
	}
}", 2, @"
class Foo
{
	void Bar (string str)
	{
		string s = str;
		string inOperator = """" + str;
	}
}");
        }

        [Test]
        public void FormatStringTests()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = string.Format(""{0}"", i.ToString());
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string s = string.Format (""{0}"", i);
	}
}");
        }

        [Test]
        public void HandlesNonLiteralFormatParameter()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string format = ""{0}"";
		string s = string.Format(format, i.ToString());
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string format = ""{0}"";
		string s = string.Format (format, i);
	}
}");
        }

        [Test]
        public void FormatStringWithNonObjectParameterTests()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat(""{0} {1}"", i.ToString(), i.ToString());
	}

	void FakeFormat(string format, string arg0, object arg1)
	{
	}
	void FakeFormat(string format, params object[] arg1)
	{
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat (""{0} {1}"", i.ToString (), i);
	}

	void FakeFormat(string format, string arg0, object arg1)
	{
	}
	void FakeFormat(string format, params object[] arg1)
	{
	}
}");
        }

        [Test]
        public void FormatMethodWithObjectParamsArray()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat(""{0} {1}"", i.ToString(), i.ToString());
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}", 2, @"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat (""{0} {1}"", i, i);
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}");
        }

        [Test, Ignore("broken")]
        public void DetectsBlacklistedCalls()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		var w = new System.IO.StringWriter ();
		w.Write (i.ToString());
		w.WriteLine (i.ToString());
	}
}", 2, @"
class Foo
{
	void Bar (object i)
	{
		var w = new System.IO.StringWriter ();
		w.Write (i);
		w.WriteLine (i);
	}
}");
        }

        [Test]
        public void ConcatenationOperator2()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = """" + i.ToString() + """" + i.ToString();
	}
}", 2, @"
class Foo
{
	void Bar (int i)
	{
		string s = """" + i + """" + i;
	}
}");
        }

        [Test]
        public void TestReferenceTypes2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = """" + i.ToString() + """" + i.ToString();
	}
}");
        }

        [Test]
        public void ConcatenationOperatorWithToStringAsOnlyString2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = i.ToString() + i + i + i + 1.3;
	}
}");
        }

        [Test]
        public void IgnoresCallsToIFormattableToString2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (System.DateTime dt)
	{
		string s = dt.ToString("""", CultureInfo.InvariantCulture) + string.Empty;
	}
}");
        }

        [Test]
        public void FormatStringTests2()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = string.Format(""{0}"", i.ToString());
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string s = string.Format (""{0}"", i);
	}
}");
        }

        [Test]
        public void HandlesNonLiteralFormatParameter2()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string format = ""{0}"";
		string s = string.Format(format, i.ToString());
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string format = ""{0}"";
		string s = string.Format (format, i);
	}
}");
        }

        [Test]
        public void FormatStringWithNonObjectParameterTests2()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat(""{0} {1}"", i.ToString(), i.ToString());
	}

	void FakeFormat(string format, string arg0, object arg1)
	{
	}
	void FakeFormat(string format, params object[] args)
	{
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat (""{0} {1}"", i.ToString (), i);
	}

	void FakeFormat(string format, string arg0, object arg1)
	{
	}
	void FakeFormat(string format, params object[] args)
	{
	}
}");
        }

        [Test]
        public void FormatMethodWithObjectParamsArray2()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat(""{0} {1}"", i.ToString(), i.ToString());
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}", 2, @"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat (""{0} {1}"", i, i);
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}");
        }

        [Test]
        public void DetectsBlacklistedCalls2()
        {
            Test<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		var w = new System.IO.StringWriter ();
		w.Write (i.ToString());
		w.WriteLine (i.ToString());
	}
}", 2, @"
class Foo
{
	void Bar (int i)
	{
		var w = new System.IO.StringWriter ();
		w.Write (i);
		w.WriteLine (i);
	}
}");
        }

    }
}

