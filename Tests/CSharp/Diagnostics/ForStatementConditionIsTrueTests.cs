using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ForStatementConditionIsTrueTests : CSharpDiagnosticTestBase
    {

        [Test]
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

        [Test]
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

        [Test]
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