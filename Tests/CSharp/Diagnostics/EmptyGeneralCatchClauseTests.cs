using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class EmptyGeneralCatchClauseTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void TestCatchWhenWithoutDeclaration()
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
				catch when (ex.Message != null)
				{
				}
			}
		}
	}
");
        }

        [Fact]
        public void TestCatchWithReturnStatement()
        {
            Analyze<EmptyGeneralCatchClauseAnalyzer>(@"using System;
	using System.IO;
	namespace Application
	{
		public class BaseClass
		{
			public bool Method()
			{
				try
				{
					return true;
				}
				catch
				{
                    return false;
				}
			}
		}
	}
");
        }

        [Fact]
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
#pragma warning disable " + CSharpDiagnosticIDs.EmptyGeneralCatchClauseAnalyzerID + @"
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