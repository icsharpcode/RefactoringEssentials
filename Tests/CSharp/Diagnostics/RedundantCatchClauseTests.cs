using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantCatchClauseTests : CSharpDiagnosticTestBase
    {
        const string BaseInput = @"
using System;
class A
{
	void F()
	{";


        [Fact(Skip="TODO: Issue not ported yet")]
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

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestEmptyCatch()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
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
}", 2, BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException aoore) {
			Console.WriteLine (aoore);
		}  
	}
}");
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestOnlyRedundantCatches()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
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

        [Fact(Skip="TODO: Issue not ported yet")]
        public void AddsBlockIfNeccessary()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
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


        [Fact(Skip="TODO: Issue not ported yet")]
        public void AddsBlockIfNeccessaryOnEmptyTryBlock()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
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

        [Fact(Skip="TODO: Issue not ported yet")]
        public void EmptyTryCatchSkeleton()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
		} catch {
		}
	}
}", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void DoesNotAddBlockIfUnneccessary()
        {
            Test<RedundantCatchClauseAnalyzer>(@"
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

        [Fact(Skip="TODO: Issue not ported yet")]
        public void NoIssuesWhenMissingCatch()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		}
	}
}", 0);
        }

        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestEmptyCatchWithFinally()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
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
        [Fact(Skip="TODO: Issue not ported yet")]
        public void TestBug12273()
        {
            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
		try {
			F ();
		} catch (ArgumentOutOfRangeException) {
			throw;
		} catch (Exception e) {
			Console.WriteLine (e);
		}
	}
}", 0);

            Test<RedundantCatchClauseAnalyzer>(BaseInput + @"
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
        [Fact(Skip="TODO: Issue not ported yet")]
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
        [Fact(Skip="TODO: Issue not ported yet")]
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

