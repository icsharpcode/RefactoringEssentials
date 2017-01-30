using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantExtendsListEntryTests : CSharpDiagnosticTestBase
    {

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase1()
        {
            Test<RedundantExtendsListEntryAnalyzer>(@"using System;

namespace resharper_test
{
	public interface interf
	{
		void method();
	}

	public class baseClass:interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}

	public partial class Foo: baseClass,interf
	{
	}
	public partial class Foo 
	{
	}
}
", @"using System;

namespace resharper_test
{
	public interface interf
	{
		void method();
	}

	public class baseClass:interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}

	public partial class Foo: baseClass
	{
	}
	public partial class Foo 
	{
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase2()
        {
            Test<RedundantExtendsListEntryAnalyzer>(@"using System;

namespace resharper_test
{
	public interface interf
	{
		void method();
	}

	public class baseClass:interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}

	public partial class Foo: baseClass
	{
	}
	public partial class Foo: baseClass
	{
	}
}
", 2, @"using System;

namespace resharper_test
{
	public interface interf
	{
		void method();
	}

	public class baseClass:interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}

	public partial class Foo 
	{
	}
	public partial class Foo 
	{
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestInspectorCase3()
        {
            Analyze<RedundantExtendsListEntryAnalyzer>(@"using System;

namespace resharper_test
{
	public interface interf
	{
		void method();
	}

	public class baseClass:interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}

	public class Foo: baseClass, interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}
}
");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestResharperDisableRestore()
        {
            Analyze<RedundantExtendsListEntryAnalyzer>(@"using System;

namespace resharper_test
{
	public interface interf
	{
		void method();
	}

	public class baseClass:interf
	{
		public void method()
		{
			throw new NotImplementedException();
		}
	}
//Resharper disable RedundantExtendsListEntry
	public class Foo: baseClass, interf
	{
	}
//Resharer restore RedundantExtendsListEntry
}
");
        }
    }
}