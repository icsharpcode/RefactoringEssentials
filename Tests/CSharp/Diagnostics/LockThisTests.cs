using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class LockThisTests : CSharpDiagnosticTestBase
    {
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockThisInMethod()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		lock (this) {
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	void TestMethod ()
	{
		lock (locker) {
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockThisInGetter()
        {
            var input = @"
class TestClass
{
	int MyProperty {
		get {
			lock (this) {
				return 0;
			}
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	int MyProperty {
		get {
			lock (locker) {
				return 0;
			}
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockThisInSetter()
        {
            var input = @"
class TestClass
{
	int MyProperty {
		set {
			lock (this) {
			}
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	int MyProperty {
		set {
			lock (locker) {
			}
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockThisInConstructor()
        {
            var input = @"
class TestClass
{
	TestClass()
	{
		lock (this) {
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	TestClass()
	{
		lock (locker) {
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockThisInDelegate()
        {
            var input = @"
class TestClass
{
	TestClass()
	{
		Action lockThis = delegate ()
		{
			lock (this) {
			}
		};
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	TestClass()
	{
		Action lockThis = delegate ()
		{
			lock (locker) {
			}
		};
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockThisInLambda()
        {
            var input = @"
class TestClass
{
	TestClass()
	{
		Action lockThis = () =>
		{
			lock (this) {
			}
		};
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	TestClass()
	{
		Action lockThis = () =>
		{
			lock (locker) {
			}
		};
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockParenthesizedThis()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		lock ((this)) {
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	void TestMethod ()
	{
		lock (locker) {
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFixMultipleLockThis()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		lock (this) {
		}
	}

	void TestMethod2 ()
	{
		lock (this) {
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	void TestMethod ()
	{
		lock (locker) {
		}
	}

	void TestMethod2 ()
	{
		lock (locker) {
		}
	}
}";

            Test<LockThisAnalyzer>(input, 2, output, 0);
        }
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestFixMixedLocks()
        {
            var input = @"
class TestClass
{
	void TestMethod ()
	{
		lock (this) {
		}
	}

	object locker2 = new object ();
	void TestMethod2 ()
	{
		lock (locker2) {
		}
	}
}";

            var output = @"
class TestClass
{
	object locker = new object ();
	void TestMethod ()
	{
		lock (locker) {
		}
	}

	object locker2 = new object ();
	void TestMethod2 ()
	{
		lock (locker2) {
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLockNonThis()
        {
            var input = @"
class TestClass
{
	object locker = new object ();

	TestClass()
	{
		lock (locker) {
		}
	}
}";

            Test<LockThisAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNestedTypeLock()
        {
            var input = @"
class TestClass
{
	class Nested
	{
		Nested()
		{
			lock (this) {
			}
		}
	}
}";

            var output = @"
class TestClass
{
	class Nested
	{
		object locker = new object ();
		Nested()
		{
			lock (locker) {
			}
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMethodSynchronized()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	void TestMethod ()
	{
		System.Console.WriteLine (""Foo"");
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	object locker = new object ();
	void TestMethod ()
	{
		lock (locker) {
			System.Console.WriteLine (""Foo"");
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMethodWithSynchronizedValue()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	[MethodImpl (Value = MethodImplOptions.Synchronized)]
	void TestMethod ()
	{
		System.Console.WriteLine (""Foo"");
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	object locker = new object ();
	void TestMethod ()
	{
		lock (locker) {
			System.Console.WriteLine (""Foo"");
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMethodHasSynchronized()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
	void TestMethod ()
	{
		System.Console.WriteLine (""Foo"");
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	object locker = new object ();
	[MethodImpl (MethodImplOptions.NoInlining)]
	void TestMethod ()
	{
		lock (locker) {
			System.Console.WriteLine (""Foo"");
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMethodNotSynchronized()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	[MethodImpl (MethodImplOptions.NoInlining)]
	void TestMethod ()
	{
		System.Console.WriteLine (""Foo"");
	}
}";

            Test<LockThisAnalyzer>(input, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestAbstractSynchronized()
        {
            var input = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	public abstract void TestMethod ();
}";

            var output = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	public abstract void TestMethod ();
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDoubleLocking()
        {
            var input = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	public void TestMethod ()
	{
		lock (this) {
		}
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	object locker = new object ();
	public void TestMethod ()
	{
		lock (locker) {
			lock (locker) {
			}
		}
	}
}";

            Test<LockThisAnalyzer>(input, 2, output, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestDelegateLocking()
        {
            var input = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	public void TestMethod ()
	{
		Action action = delegate {
			lock (this) {
			}
		};
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	object locker = new object ();
	public void TestMethod ()
	{
		lock (locker) {
			Action action = delegate {
				lock (locker) {
				}
			};
		}
	}
}";

            Test<LockThisAnalyzer>(input, 2, output, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestLambdaLocking()
        {
            var input = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	public void TestMethod ()
	{
		Action action = () => {
			lock (this) {
			}
		};
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
abstract class TestClass
{
	object locker = new object ();
	public void TestMethod ()
	{
		lock (locker) {
			Action action = () => {
				lock (locker) {
				}
			};
		}
	}
}";

            Test<LockThisAnalyzer>(input, 2, output, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestStaticMethod()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	public static void TestMethod ()
	{
		Console.WriteLine ();
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	static object locker = new object ();
	public static void TestMethod ()
	{
		lock (locker) {
			Console.WriteLine ();
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestStaticProperty()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	public static int TestProperty
	{
		[MethodImpl (MethodImplOptions.Synchronized)]
		set {
			Console.WriteLine (value);
		}
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	static object locker = new object ();
	public static int TestProperty
	{
		set {
			lock (locker) {
				Console.WriteLine (value);
			}
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestMixedStaticMethod()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	[MethodImpl (MethodImplOptions.Synchronized)]
	public void TestMethod ()
	{
		Console.WriteLine ();
	}

	[MethodImpl (MethodImplOptions.Synchronized)]
	public static void TestStaticMethod ()
	{
		Console.WriteLine ();
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	object locker = new object ();
	public void TestMethod ()
	{
		lock (locker) {
			Console.WriteLine ();
		}
	}

	[MethodImpl (MethodImplOptions.Synchronized)]
	public static void TestStaticMethod ()
	{
		Console.WriteLine ();
	}
}";

            Test<LockThisAnalyzer>(input, 2, output, 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestNewNameLock()
        {
            var input = @"
using System.Runtime.CompilerServices;
class TestClass
{
	int locker;
	[MethodImpl (MethodImplOptions.Synchronized)]
	public void TestMethod ()
	{
		Console.WriteLine ();
	}
}";

            var output = @"
using System.Runtime.CompilerServices;
class TestClass
{
	int locker;
	object locker1 = new object ();
	public void TestMethod ()
	{
		lock (locker1) {
			Console.WriteLine ();
		}
	}
}";

            Test<LockThisAnalyzer>(input, 1, output);
        }
    }
}
