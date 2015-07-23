using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class RedundantCatchClauseTests : CSharpDiagnosticTestBase
    {
        const string BaseInput = @"
using System;
class A
{
	void F()
	{";


        [Test]
        public void TestDisable()
        {
            var input = BaseInput + @"
// ReSharper disable once RedundantCatchClause
		try {
			F ();
		} catch {
			throw;
		} finally {
			Console.WriteLine (""Inside finally"");
		}
	}
}";
            Analyze<RedundantCatchClauseAnalyzer>(input);
        }

        [Test]
        public void TestEmptyCatch()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException aoore) {
			Console.WriteLine (aoore);
		} catch (ArgumentException) {
			throw;
		} catch {
			throw;
		}
	}
}", BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException aoore) {
			Console.WriteLine (aoore);
		}  
	},
}",2);
        }

        [Test]
        public void TestOnlyRedundantCatches()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
			Console.WriteLine (""Inside try"");
		} catch {
			throw;
		}
	}
}", BaseInput + @"
		F ();
		Console.WriteLine (""Inside try"");
	}
}");
        }

        [Test]
        public void AddsBlockIfNeccessary()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		if (true)
			try {
				F ();
				Console.WriteLine (""Inside try"");
			} catch {
				throw;
			}
	}
}", BaseInput + @"
		if (true) {
			F ();
			Console.WriteLine (""Inside try"");
		}
	}
}");
        }


        [Test]
        public void AddsBlockIfNeccessaryOnEmptyTryBlock()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		if (true)
			try {
			} catch {
				throw;
			}
	}
}", BaseInput + @"
		if (true) {
		}
	}
}");
        }

        [Test]
        public void EmptyTryCatchSkeleton()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
		} catch {
		}
	}
}");
        }

        [Test]
        public void DoesNotAddBlockIfUnneccessary()
        {
            Analyze<RedundantCatchClauseAnalyzer>(@"
		if (true)
			try {
				F ();
			} catch {
				throw;
			}
	}
}", BaseInput + @"
		if (true)
			F ();
	}
}");
        }

        [Test]
        public void NoIssuesWhenMissingCatch()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		}
	}
}");
        }

        [Test]
        public void TestEmptyCatchWithFinally()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch {
			throw;
		} finally {
			Console.WriteLine (""Inside finally"");
		}
	}
}", BaseInput + @"
		try {
			F ();
		}  finally {
			Console.WriteLine (""Inside finally"");
		}
	}
}");
        }

        /// <summary>
        /// Bug 12273 - Incorrect redundant catch warning
        /// </summary>
        [Test]
        public void TestBug12273()
        {
            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException) {
			throw;
		} catch (Exception e) {
			Console.WriteLine (e);
		}
	}
}");

            Analyze<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException) {
			throw;
		} catch (Exception e) {
			throw;
		}
	}
}", BaseInput + @"
		F ();
	}
}");

        }

        /// <summary>
        /// Bug 12273 - Incorrect redundant catch warning
        /// </summary>
        [Test]
        public void TestBug12273Case2()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException) {
			throw;
		} catch {
			Console.WriteLine (""hello world"");
		}
	}
}", 0);

            TestIssue<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException) {
			throw;
		} catch {
			throw;
		}
	}
}");
        }

        /// <summary>
        /// Bug 14451 - False positive of "Redundant catch clause" 
        /// </summary>
        [Test]
        public void TestBugBug14451()
        {
            Analyze<RedundantCatchClauseAnalyzer>(@"
using System;
public class Test {
    public void Foo() {
        try {
            Foo();
        }
        catch (Exception ex) {
            throw new Exception(""Some additional information: "" + ex.Message, ex);
        }
    }
}
");
        }
    }
}

