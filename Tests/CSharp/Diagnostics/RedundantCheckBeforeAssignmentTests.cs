using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantCheckBeforeAssignmentTests : CSharpDiagnosticTestBase
    {

        [Test]
        public void TestInspectorCase1()
        {
            Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class baseClass
{
	public void method()
	{
		int q = 1;
		if (q != 1) {
			q = 1;
		}
	}
}
", @"using System;
class baseClass
{
	public void method()
	{
		int q = 1;
		q = 1;
	}
}
");
        }

        [Test]
        public void TestInspectorCase2()
        {
            TestIssue<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			int q = 1;
			if (q != 1)
				q = 1;
		}
	}
}
");
        }

        [Test]
        public void TestInspectorCase3()
        {
            Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			int q = 1;
			if (1+0 != q)
			{
				q = 1 + 0;
			}
			else
			{}
		}
	}
}
");
        }

        [Test]
        public void TestInspectorCase4()
        {
            Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			int q = 1;
			if (1+0 != q)
			{
				q = 1 + 0;
			}
			else if(true)
			{}
		}
	}
}
");
        }

        [Test]
        public void TestResharperDisableRestore()
        {
            Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
namespace resharper_test
{
	public class baseClass
	{
		public void method()
		{
			int q = 1;
//Resharper disable RedundantCheckBeforeAssignment
			if (q != 1)
			{
				q = 1;
			}
//Resharper restore RedundantCheckBeforeAssignment
		}
	}
}
");
        }
    }
}