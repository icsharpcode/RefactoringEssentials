using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

class Test
{
    readonly object fooBar = new object();

    public Test()
    {
    }
}

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class CS0169FieldIsNeverUsedTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestInitializedField()
        {
            Analyze<CS0169FieldIsNeverUsedAnalyzer>(@"class Test
{
    object fooBar = new object ();
    public static void Main (string[] args)
    {
        Console.WriteLine (fooBar);
    }
}");
        }

        [Test]
        public void TestFieldAssignedInConstructor()
        {
            Analyze<CS0169FieldIsNeverUsedAnalyzer>(@"class Test
{
	object fooBar;
	public Test ()
	{
		fooBar = new object ();
	}
	public static void Main (string[] args)
	{
		Console.WriteLine (fooBar);
	}
}");
        }

        [Test]
        public void TestUnassignedField()
        {
            TestIssue<CS0169FieldIsNeverUsedAnalyzer>(@"class Test
{
	object fooBar;
}");
        }



        [Test]
        public void TestFieldOnlyUsed()
        {
            TestIssue<CS0169FieldIsNeverUsedAnalyzer>(@"class Test
{
	object fooBar;
	public Test ()
	{
		Console.WriteLine (fooBar);
	}
}");
        }


        [Test]
        public void TestReadonlyField()
        {
            TestIssue<CS0169FieldIsNeverUsedAnalyzer>(@"class Test
{
	readonly object fooBar;
}");
        }


        [Test]
        public void TestPragmaDisable()
        {
            Analyze<CS0169FieldIsNeverUsedAnalyzer>(@"class Test
{
#pragma warning disable 169
	object fooBar;
#pragma warning restore 169
	public Test ()
	{
		Console.WriteLine (fooBar);
	}
}");
        }
    }
}

