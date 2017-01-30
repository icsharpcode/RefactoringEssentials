using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantBaseQualifierTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<RedundantBaseQualifierAnalyzer>(@"using System;
	namespace Application
	{
		public class BaseClass
		{
			public int a;
			public virtual void method()
			{
				Console.Write(Environment.NewLine);
			}
			public void method1()
			{
				Console.Write(Environment.NewLine);
			}
		}

		class Program : BaseClass
		{
			public void method2(int a)
			{
				base.a = 1;
			}
			public override void method()
			{
				$base.$method1();
			}
		}
	}
", @"using System;
	namespace Application
	{
		public class BaseClass
		{
			public int a;
			public virtual void method()
			{
				Console.Write(Environment.NewLine);
			}
			public void method1()
			{
				Console.Write(Environment.NewLine);
			}
		}

		class Program : BaseClass
		{
			public void method2(int a)
			{
				base.a = 1;
			}
			public override void method()
			{
				method1();
			}
		}
	}
");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<RedundantBaseQualifierAnalyzer>(@"using System;
	namespace Application
	{
		public class BaseClass
		{
			public int a;
			public int b;
			public void method()
			{
				Console.Write(Environment.NewLine);
			}
			public virtual void method1()
			{
				Console.Write(Environment.NewLine);
			}
		}

		class Program : BaseClass
		{
			public new void method(int b)
			{
				base.a = 1;
				base.b = 2;
			}
			public void method2()
			{
				$base.$method ();
			}
			public new int a;
		}
	}
", @"using System;
	namespace Application
	{
		public class BaseClass
		{
			public int a;
			public int b;
			public void method()
			{
				Console.Write(Environment.NewLine);
			}
			public virtual void method1()
			{
				Console.Write(Environment.NewLine);
			}
		}

		class Program : BaseClass
		{
			public new void method(int b)
			{
				base.a = 1;
				base.b = 2;
			}
			public void method2()
			{
				method ();
			}
			public new int a;
		}
	}
");
        }

        [Fact]
        public void ComplexTests()
        {
            Analyze<RedundantBaseQualifierAnalyzer>(@"

class Base {
	public int a;
}

class Foo : Base
{
	void Bar ()
	{
		{
			int a = 343;
		}
		base.a = 2;
		
	}

	void FooBar ()
	{
		int a = base.a;
	}
}");
        }

        [Fact]
        public void InvalidUseOfBaseInFieldInitializer()
        {
            var input = @"class Foo
{
	int a = base.a;
}";
            Analyze<RedundantBaseQualifierAnalyzer>(input);
        }
    }
}