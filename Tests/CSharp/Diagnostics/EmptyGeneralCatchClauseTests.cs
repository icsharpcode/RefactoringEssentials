using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class EmptyGeneralCatchClauseTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInspectorCase1()
        {
            Analyze<EmptyGeneralCatchClauseAnalyzer>(@"using System;
	using System.IO;
	namespace Application
	{
		public class BaseClass
		{
			public void method()
			{
				try
				{
					F ();
				}
				$catch$ (Exception e)
				{
				}
			}
		}
	}
");
        }

        [Test]
        public void TestInspectorCase2()
        {
            Analyze<EmptyGeneralCatchClauseAnalyzer>(@"using System;
	using System.IO;
	namespace Application
	{
		public class BaseClass
		{
			public void method()
			{
				try
				{
					F ();
				}
				$catch$
				{
				}
			}
		}
	}
");
        }

        [Test]
        public void TestCatchWhen()
        {
            Analyze<EmptyGeneralCatchClauseAnalyzer>(@"using System;
	using System.IO;
	namespace Application
	{
		public class BaseClass
		{
			public void method()
			{
				try
				{
					F ();
				}
				catch (Exception ex) when (ex.Message != null)
				{
				}
			}
		}
	}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<EmptyGeneralCatchClauseAnalyzer>(@"using System;
	using System.IO;
	namespace Application
	{
		public class BaseClass
		{
			public void method()
			{
				try
				{
					F ();
				}
#pragma warning disable " + DiagnosticIDs.EmptyGeneralCatchClauseAnalyzerID + @"
				catch (Exception e)
				{
				}
			}
		}
	}
");
        }
    }
}