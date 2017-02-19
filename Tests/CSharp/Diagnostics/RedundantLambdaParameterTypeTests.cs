using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantLambdaParameterTypeTests : CSharpDiagnosticTestBase
    {

        [Fact(Skip="TODO: Issue not ported yet")]
        public void SimpleCase()
        {
            Test<RedundantLambdaParameterTypeAnalyzer>(@"
class Program
{
	public delegate int IncreaseByANumber(int j);

	public static void ExecuteCSharp3_0()
	{
		IncreaseByANumber increase = (int j) => (j * 42);
	}
}
", @"
class Program
{
	public delegate int IncreaseByANumber(int j);

	public static void ExecuteCSharp3_0()
	{
		IncreaseByANumber increase = j => (j * 42);
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void MultipleCases()
        {
            Test<RedundantLambdaParameterTypeAnalyzer>(@"
class Program
{
	public delegate int MultipleIncreaseByANumber(int i, string j, int l);

	public static void ExecuteCSharp3_0()
	{
		MultipleIncreaseByANumber multiple = (int j, string k, int l) => ((j * 42) / k) % l;
	}
}
", 3, @"
class Program
{
	public delegate int MultipleIncreaseByANumber(int i, string j, int l);

	public static void ExecuteCSharp3_0()
	{
		MultipleIncreaseByANumber multiple = (j, k, l) => ((j * 42) / k) % l;
	}
}
", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase2()
        {
            Test<RedundantLambdaParameterTypeAnalyzer>(@"
using System;
using System.Collections.Generic;
using System.Linq;

namespace application
{
	internal class Program
	{
		public void Foo(Action<int, string> act) {}
		public void Foo(Action<int> act) {}

		void Test ()
		{
			Foo((int i) => Console.WriteLine (i));
		}
	}
}", @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace application
{
	internal class Program
	{
		public void Foo(Action<int, string> act) {}
		public void Foo(Action<int> act) {}

		void Test ()
		{
			Foo(i => Console.WriteLine (i));
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase3()
        {
            Analyze<RedundantLambdaParameterTypeAnalyzer>(@"using System;
using System.Collections.Generic;
using System.Linq;

namespace application
{
	internal class Program
	{
		public void Foo(Action<int> act, Action<int> act1) { }
		public void Foo(Action<string> act, Action<int> act1) { }

		void Test()
		{
			Foo(((int i) => Console.WriteLine(i)), (i => Console.WriteLine(i)));
		}
	}
}");
        }


        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInvalidContext()
        {
            Analyze<RedundantLambdaParameterTypeAnalyzer>(@"using System;
		using System.Collections.Generic;
		using System.Linq;

namespace application
{
	internal class Program
	{
		public void Foo(Action<int> act) {}
		public void Foo(Action<string> act) {}

		void Test ()
		{
			Foo((int i) => Console.WriteLine (i));
		}
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestResharperDisableRestore()
        {
            Analyze<RedundantLambdaParameterTypeAnalyzer>(@"using System;
		using System.Collections.Generic;
		using System.Linq;

namespace application
{
	internal class Program
	{
		public delegate int IncreaseByANumber(int j);

		public delegate int MultipleIncreaseByANumber(int i, int j, int l);

		public static void ExecuteCSharp3_0()
		{
			// declare the lambda expression
			//Resharper disable RedundantLambdaParameterType
			IncreaseByANumber increase = (int j) => (j * 42);
			//Resharper restore RedundantLambdaParameterType
			// invoke the method and print 420 to the console
			Console.WriteLine(increase(10));

			MultipleIncreaseByANumber multiple = (j, k, l) => ((j * 42) / k) % l;
			Console.WriteLine(multiple(10, 11, 12));
		}
	}
}");
        }
    }
}