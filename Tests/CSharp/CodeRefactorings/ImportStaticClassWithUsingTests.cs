using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ImportStaticClassWithUsingTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimple()
        {
            Test<ImportStaticClassWithUsingCodeRefactoringProvider>(@"
using System;

class Foo
{
    public void Test()
    {
        $Math.Sin(0);
    }
}", @"
using System;
using static System.Math;

class Foo
{
    public void Test()
    {
        Sin(0);
    }
}");
        }

        [Test]
        public void TestExtensionMethod()
        {
            TestWrongContext<ImportStaticClassWithUsingCodeRefactoringProvider>(@"
using System;

class Foo
{
    public void Test()
    {
        int[] array = new[] { 0, 1, 2 };
        int? first = System.Linq.$Enumerable.FirstOrDefault(array);
    }
}");
        }

        [Test]
        public void TestMemberConflict()
        {
            Test<ImportStaticClassWithUsingCodeRefactoringProvider>(@"
using System;

class Foo
{
    public void Test()
    {
        $Math.Sin(0);
        Math.Tan(0);
    }
    public void Tan(int i)
    {
    }
}", @"
using System;
using static System.Math;

class Foo
{
    public void Test()
    {
        Sin(0);
        Math.Tan(0);
    }
    public void Tan(int i)
    {
    }
}");
        }

        [Test]
        public void TestLocalConflict()
        {
            Test<ImportStaticClassWithUsingCodeRefactoringProvider>(@"
using System;

class Foo
{
    public void Test()
    {
        $Math.Sin(0);
        Action<int> Tan = i => i;
        Math.Tan(0);
    }
}", @"
using System;
using static System.Math;

class Foo
{
    public void Test()
    {
        Sin(0);
        Action<int> Tan = i => i;
        Math.Tan(0);
    }
}");
        }

    }
}