using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ImportStaticClassWithUsingTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

        [Fact]
        public void TestStaticClassInSameCompilationUnit1()
        {
            Test<ImportStaticClassWithUsingCodeRefactoringProvider>(@"
using System;
using Namespace1.Namespace2;

namespace Namespace1.Namespace2
{
    public static class StaticLinqClass
    {
        public static int? First(System.Collections.Generic.IEnumerable<int> enumerable)
        {
            return null;
        }
    }
}

class Foo
{
    public void Test()
    {
        var f = $StaticLinqClass.First(new[] { 1 });
    }
}", @"
using System;
using Namespace1.Namespace2;
using static Namespace1.Namespace2.StaticLinqClass;

namespace Namespace1.Namespace2
{
    public static class StaticLinqClass
    {
        public static int? First(System.Collections.Generic.IEnumerable<int> enumerable)
        {
            return null;
        }
    }
}

class Foo
{
    public void Test()
    {
        var f = First(new[] { 1 });
    }
}");
        }

        [Fact]
        public void TestStaticClassInSameCompilationUnit2()
        {
            Test<ImportStaticClassWithUsingCodeRefactoringProvider>(@"
using System;

static class StaticLinqClass
{
    public static int? First(System.Collections.Generic.IEnumerable<int> enumerable)
    {
        return null;
    }
}

class Foo
{
    public void Test()
    {
        var f = $StaticLinqClass.First(new[] { 1 });
    }
}", @"
using System;
using static StaticLinqClass;

static class StaticLinqClass
{
    public static int? First(System.Collections.Generic.IEnumerable<int> enumerable)
    {
        return null;
    }
}

class Foo
{
    public void Test()
    {
        var f = First(new[] { 1 });
    }
}");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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