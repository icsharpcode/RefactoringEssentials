using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ConvertNullableToShortFormTests : CSharpDiagnosticTestBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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


        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
