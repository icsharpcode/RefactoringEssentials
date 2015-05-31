using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class OperatorIsCanBeUsedTests : CSharpDiagnosticTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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