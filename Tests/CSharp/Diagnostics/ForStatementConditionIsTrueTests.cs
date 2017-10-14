using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ForStatementConditionIsTrueTests : CSharpDiagnosticTestBase
    {

        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<ForStatementConditionIsTrueAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			for (; $true$ ;)
			{}
		}
	}
}
", @"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			for (; ;)
			{}
		}
	}
}
");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<ForStatementConditionIsTrueAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			for (int a; $true$ ; )
			{}
		}
	}
}
", @"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			for (int a; ; )
			{}
		}
	}
}
");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<ForStatementConditionIsTrueAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
#pragma warning disable " + CSharpDiagnosticIDs.ForStatementConditionIsTrueAnalyzerID + @"
			for (; true ;)
			{}
		}
	}
}
");
        }
    }
}