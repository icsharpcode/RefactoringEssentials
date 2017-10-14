using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class OperatorIsCanBeUsedTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<OperatorIsCanBeUsedAnalyzer>(@"
using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		public static void main(string[] args)
		{
			int a = 1;
			if ($typeof(int) == a.GetType()$) {
			}
		}
	}
}", @"
using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		public static void main(string[] args)
		{
			int a = 1;
			if (a is int) {
			}
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<OperatorIsCanBeUsedAnalyzer>(@"
using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		public static void main(string[] args)
		{
			int a = 1;
			if ($a.GetType() == typeof(int)$) {
			}
		}
	}
}", @"
using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		public static void main(string[] args)
		{
			int a = 1;
			if (a is int) {
			}
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase3()
        {
            Analyze<OperatorIsCanBeUsedAnalyzer>(@"
using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		static public int a;
		public static void main(string[] args)
		{
			if ($BaseClass.a.GetType() == typeof(int)$) {
			}
		}
	}
}", @"
using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		static public int a;
		public static void main(string[] args)
		{
			if (BaseClass.a is int) {
			}
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase4()
        {
            Analyze<OperatorIsCanBeUsedAnalyzer>(@"
using System;
using System.Reflection;

namespace Demo
{
	public sealed class TestClass
	{
	}

	public class BaseClass 
	{
		public static void main(string[] args)
		{
			BaseClass b = new BaseClass();
			if ($typeof(TestClass) == b.GetType()$) {
			}
		}
	}
}", @"
using System;
using System.Reflection;

namespace Demo
{
	public sealed class TestClass
	{
	}

	public class BaseClass 
	{
		public static void main(string[] args)
		{
			BaseClass b = new BaseClass();
			if (b is TestClass) {
			}
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase5()
        {
            Analyze<OperatorIsCanBeUsedAnalyzer>(@"
using System;
using System.Reflection;

namespace Demo
{
	public class TestClass
	{
	}

	public class BaseClass : TestClass
	{
		public static void main(string[] args)
		{
			BaseClass b = new BaseClass();
			if ((typeof(TestClass) == b.GetType())) {

			}
		}
	}
}");
        }

        [Fact]
        public void TestResharperDisable()
        {
            Analyze<OperatorIsCanBeUsedAnalyzer>(@"using System;
using System.Linq;
using System.Reflection;

namespace Demo
{
	public class BaseClass
	{
		public static void main(string[] args)
		{
			int a = 1;
#pragma warning disable " + CSharpDiagnosticIDs.OperatorIsCanBeUsedAnalyzerID + @"
			if ((typeof (int) == a.GetType())) {
			}
		}
	}
}");
        }
    }
}