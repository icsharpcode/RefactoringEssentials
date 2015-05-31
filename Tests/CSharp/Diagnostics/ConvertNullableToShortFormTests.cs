using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ConvertNullableToShortFormTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"using System;

class Foo
{
    $Nullable<int>$ Bar()
    {
        return 5;
    }
}", @"using System;

class Foo
{
    int? Bar()
    {
        return 5;
    }
}");
        }

        [Test]
        public void TestSimpleCaseWithXmlDoc()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"using System;

class Foo
{
    /// <summary>
    /// Method description.
    /// </summary>
    $Nullable<int>$ Bar()
    {
        return 5;
    }
}", @"using System;

class Foo
{
    /// <summary>
    /// Method description.
    /// </summary>
    int? Bar()
    {
        return 5;
    }
}");
        }

        [Test]
        public void TestFullyQualifiedNameCase()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"class Foo
{
    void Bar()
    {
        $System.Nullable<int>$ a;
    }
}", @"class Foo
{
    void Bar()
    {
        int? a;
    }
}");
        }


        [Test]
        public void TestAlreadyShort()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"class Foo
{
    int? Bar(int o)
    {
        return 5;
    }
}");
        }

        [Test]
        public void TestInvalid()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"using System;
namespace NN {
    class Nullable<T> {}
    class Foo
    {
        void Bar()
        {
            Nullable<int> a;
        }
    }
}");
        }

        [Test]
        public void TestInvalidTypeOf()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"using System;
class Foo
{
    bool Bar(object o)
    {
        return o.GetType() == typeof(Nullable<>);
    }
    bool Bar2(object o)
    {
        return o.GetType() == typeof(System.Nullable<>);
    }
}
");
        }

        [Test]
        public void TestDisable()
        {
            Analyze<ConvertNullableToShortFormAnalyzer>(@"class Foo
{
    void Bar()
    {
#pragma warning disable " + CSharpDiagnosticIDs.ConvertNullableToShortFormAnalyzerID + @"
        System.Nullable<int> a;
    }
}");
        }
    }
}
