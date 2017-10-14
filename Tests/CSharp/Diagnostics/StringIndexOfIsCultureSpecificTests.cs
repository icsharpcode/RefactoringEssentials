using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class StringIndexOfIsCultureSpecificTests : CSharpDiagnosticTestBase
    {
        const string stringIndexOfStringCalls = @"
using System.Collections.Generic;
class Test {
    public void StringIndexOfStringCalls(List<string> list)
    {
        $list[0].IndexOf("".com"")$;
        $list[0].IndexOf("".com"", 0)$;
        $list[0].IndexOf("".com"", 0, 5)$;
        $list[0].IndexOf(list[1], 0, 10)$;
    }
}";
        const string stringIndexOfStringCallsWithComparison = @"
using System.Collections.Generic;
class Test {
    public void StringIndexOfStringCalls(List<string> list)
    {
        list[0].IndexOf("".com"", System.StringComparison.Ordinal);
        list[0].IndexOf("".com"", 0, System.StringComparison.Ordinal);
        list[0].IndexOf("".com"", 0, 5, System.StringComparison.Ordinal);
        list[0].IndexOf(list[1], 0, 10, System.StringComparison.Ordinal);
    }
}";

        [Fact]
        public void IndexOfStringCalls()
        {
            Analyze<StringIndexOfIsCultureSpecificAnalyzer>(stringIndexOfStringCalls, stringIndexOfStringCallsWithComparison);
        }

        [Fact]
        public void IndexOfStringCallsAlreadyWithComparison()
        {
            Analyze<StringIndexOfIsCultureSpecificAnalyzer>(stringIndexOfStringCallsWithComparison);
        }

        [Fact]
        public void StringIndexOfChar()
        {
            string program = @"using System;
class Test {
	void M(string text) {
		text.IndexOf('.');
	}
}";
            Analyze<StringIndexOfIsCultureSpecificAnalyzer>(program);
        }

        [Fact]
        public void ListIndexOf()
        {
            string program = @"using System.Collections.Generic;
class Test {
	void M(List<string> list) {
		list.IndexOf("".com"");
	}
}";
            Analyze<StringIndexOfIsCultureSpecificAnalyzer>(program);
        }


        [Fact]
        public void TestDisable()
        {
            Analyze<StringIndexOfIsCultureSpecificAnalyzer>(@"using System;
using System.Collections.Generic;
class Test {
	public void StringIndexOfStringCalls(List<string> list)
	{
#pragma warning disable " + CSharpDiagnosticIDs.StringIndexOfIsCultureSpecificAnalyzerID + @"
		list[0].IndexOf("".com"");
	}
}");
        }


    }
}
