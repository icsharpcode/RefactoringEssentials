using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantToStringCallTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void ConcatenationOperator()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = """" + i$.ToString()$ + """" + i$.ToString()$;
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string s = """" + i + """" + i.ToString();
	}
}", 0);
        }

        [Test]
        public void TestValueTypes()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = """" + i$.ToString()$ + """" + i$.ToString()$;
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
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (string str)
	{
		string s = str$.ToString()$;
		string inOperator = """" + str$.ToString()$;
	}
}", @"
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
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = string.Format(""{0}"", i$.ToString()$);
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string s = string.Format(""{0}"", i);
	}
}");
        }

        [Test]
        public void HandlesNonLiteralFormatParameter()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string format = ""{0}"";
		string s = string.Format(format, i$.ToString()$);
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string format = ""{0}"";
		string s = string.Format(format, i);
	}
}");
        }

        [Ignore("Not supported")]
        [Test]
        public void FormatStringWithNonObjectParameterTests()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat(""{0} {1}"", i.ToString(), i$.ToString()$);
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
		string s = FakeFormat(""{0} {1}"", i.ToString (), i);
	}

	void FakeFormat(string format, string arg0, object arg1)
	{
	}
	void FakeFormat(string format, params object[] arg1)
	{
	}
}");
        }

        [Ignore("Not supported")]
        [Test]
        public void FormatMethodWithObjectParamsArray()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat(""{0} {1}"", i$.ToString()$, i$.ToString()$);
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		string s = FakeFormat(""{0} {1}"", i, i);
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}");
        }

        [Test]
        public void DetectsBlacklistedCalls()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		var w = new System.IO.StringWriter ();
		w.Write(i$.ToString()$);
		w.WriteLine(i$.ToString()$);
	}
}", @"
class Foo
{
	void Bar (object i)
	{
		var w = new System.IO.StringWriter ();
		w.Write(i);
		w.WriteLine(i);
	}
}");
        }

        [Test]
        public void ConcatenationOperator2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = """" + i$.ToString()$ + """" + i$.ToString()$;
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string s = """" + i.ToString() + """" + i;
	}
}", 1);
        }

        [Test]
        public void TestReferenceTypes2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (object i)
	{
		string s = """" + i$.ToString()$ + """" + i$.ToString()$;
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
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = string.Format(""{0}"", i$.ToString()$);
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string s = string.Format(""{0}"", i);
	}
}");
        }

        [Test]
        public void HandlesNonLiteralFormatParameter2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string format = ""{0}"";
		string s = string.Format(format, i$.ToString()$);
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string format = ""{0}"";
		string s = string.Format(format, i);
	}
}");
        }

        [Ignore("Not supported")]
        [Test]
        public void FormatStringWithNonObjectParameterTests2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat(""{0} {1}"", i.ToString(), i$.ToString()$);
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
		string s = FakeFormat(""{0} {1}"", i.ToString (), i);
	}

	void FakeFormat(string format, string arg0, object arg1)
	{
	}
	void FakeFormat(string format, params object[] args)
	{
	}
}");
        }

        [Ignore("Not supported")]
        [Test]
        public void FormatMethodWithObjectParamsArray2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat(""{0} {1}"", i$.ToString()$, i$.ToString()$);
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		string s = FakeFormat(""{0} {1}"", i, i);
	}

	void FakeFormat(string format, params object[] args)
	{
	}
}");
        }

        [Test]
        public void DetectsBlacklistedCalls2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
	void Bar (int i)
	{
		var w = new System.IO.StringWriter ();
		w.Write(i$.ToString()$);
		w.WriteLine(i$.ToString()$);
	}
}", @"
class Foo
{
	void Bar (int i)
	{
		var w = new System.IO.StringWriter ();
		w.Write(i);
		w.WriteLine(i);
	}
}");
        }

        /// <summary>
        /// Bug 39162 - Incorrect "Redundant ToString() call"
        /// </summary>
        [Test]
        public void TestBug39162()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
    void Bar (object i)
    {
        string s = i != null ? i.ToString()  : ""Foo"";
    }
}");
        }


        [Test]
        public void TestNoRedundantParameter()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
    void Bar (object i)
    {
        Foo(i.ToString());
    }

    void Foo (string s) {}
}");
        }

    }
}