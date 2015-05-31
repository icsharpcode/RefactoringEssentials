using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class RedundantExtendsListEntryTests : CSharpDiagnosticTestBase
    {

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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