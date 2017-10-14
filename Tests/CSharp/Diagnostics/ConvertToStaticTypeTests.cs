using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertToStaticTypeTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestInspectorCase1()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	public sealed class $TestClass$
	{
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}", @"
using System;

namespace Demo
{
	public static class TestClass
	{
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase1WithXmlDoc()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	/// <summary>
	/// Class description.
	/// </summary>
	sealed class $TestClass$
	{
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}", @"
using System;

namespace Demo
{
	/// <summary>
	/// Class description.
	/// </summary>
	static class TestClass
	{
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase2()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	public sealed class TestClass
	{
		public int B;
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}");
        }

        [Fact]
        public void TestInspectorCase3()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	public static class TestClass
	{
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}
");
        }

        [Fact]
        public void TestInspectorCase4()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	public class TestClass
	{
		TestClass(){}
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
}
");
        }


        [Fact]
        public void TestEntryPoint()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	public sealed class TestClass
	{
		static public int A;
		public static int Main()
		{
			return A + 1;
		}
	}
}
");
        }

        [Fact]
        public void TestAbstract()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
using System;

namespace Demo
{
	public abstract class TestClass
	{
		public static int Main()
		{
			return 1;
		}
	}
}
");
        }


        [Fact]
        public void TestResharperDisable()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"using System;

namespace Demo
{
//Resharper disable ConvertToStaticType
	public class TestClass
	{
		TestClass(){}
		static public int A;
		static public int ReturnAPlusOne()
		{
			return A + 1;
		}
	}
//Resharper restore ConcertToStaticType
}
");
        }

        /// <summary>
        /// Bug 16844 - Convert class to static 
        /// </summary>
        [Fact]
        public void TestBug16844()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
class ShouldBeStatic
{
    static void Func ()
    {
    }

    class OtherNotStatic
    {
    }
}
");
        }

        [Fact]
        public void TestEmptyPublicClass()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
public class ShouldNotBeStatic
{
}
");
        }

        [Fact]
        public void TestClassImplementingAnotherType()
        {
            Analyze<ConvertToStaticTypeAnalyzer>(@"
interface SomeInterface
{
}

public class ShouldNotBeStatic : SomeInterface
{
    private static string test;
}
");
        }
    }
}