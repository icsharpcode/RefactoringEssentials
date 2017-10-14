using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantToStringCallTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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


        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact(Skip="Not supported")]
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

        [Fact(Skip="Not supported")]
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

        [Fact]
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

        [Fact]
        public void ConcatenationOperator2()
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void FormatStringTests2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
    void Bar (int i)
    {
        string s = string.Format(""{0}"", i.ToString());
    }
}");
        }

        [Fact]
        public void HandlesNonLiteralFormatParameter2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
    void Bar (int i)
    {
        string format = ""{0}"";
        string s = string.Format(format, i.ToString());
    }
}");
        }

        [Fact(Skip="Not supported")]
        public void FormatStringWithNonObjectParameterTests2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
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
}");
        }

        [Fact(Skip="Not supported")]
        public void FormatMethodWithObjectParamsArray2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
    void Bar (int i)
    {
        string s = FakeFormat(""{0} {1}"", i.ToString(), i.ToString());
    }

    void FakeFormat(string format, params object[] args)
    {
    }
}");
        }

        [Fact]
        public void DetectsBlacklistedCalls2()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class Foo
{
    void Bar (int i)
    {
        var w = new System.IO.StringWriter ();
        w.Write(i.ToString());
        w.WriteLine(i.ToString());
    }
}");
        }

        /// <summary>
        /// Bug 39162 - Incorrect "Redundant ToString() call"
        /// </summary>
        [Fact]
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


        [Fact]
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

        [Fact]
        public void TestIgnoresShadowedToStringInConcatenation()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class ClassShadowingToString
{
    public new string ToString()
    {
        return ""ANYTHING"";
    }
}

class Foo
{
    void Bar (ClassShadowingToString v)
    {
        string s = """" + v.ToString();
    }
}");
        }

        [Fact]
        public void TestIgnoresShadowedToStringInFormatParameter()
        {
            Analyze<RedundantToStringCallAnalyzer>(@"
class ClassShadowingToString
{
    public new string ToString()
    {
        return ""ANYTHING"";
    }
}

class Foo
{
    void Bar (ClassShadowingToString v)
    {
        string s = string.Format(""{0}"", v.ToString());
    }
}");
        }
    }
}