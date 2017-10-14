using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class RemoveCatchExceptionTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void RemovesSimpleExceptionMatch()
        {
            Test<RemoveCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try {
        } $catch (System.Exception) {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try {
        } catch {
        }
    }
}");
        }

        [Fact]
        public void PreservesBody()
        {
            Test<RemoveCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try {
        } $catch (System.Exception e) {
            System.Console.WriteLine (""Hi"");
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try {
        } catch {
            System.Console.WriteLine (""Hi"");
        }
    }
}");
        }

        [Fact]
        public void PreservesWhitespaceInBody1()
        {
            Test<RemoveCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try {
        }
        $catch (System.Exception e) {

        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try {
        }
        catch {

        }
    }
}");
        }

        [Fact]
        public void PreservesWhitespaceInBody2()
        {
            Test<RemoveCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try {
        }
        $catch (System.Exception e)
        {

        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try {
        }
        catch
        {

        }
    }
}");
        }

        [Fact]
        public void IgnoresReferencedExceptionMatch()
        {
            TestWrongContext<RemoveCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {    
        try {
        } $catch (System.Exception e) {
            System.Console.WriteLine (e);
        }
    }
}");
        }

        [Fact]
        public void TestNullReferenceExceptionBig()
        {
            TestWrongContext<RemoveCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {    
        try {
        } $catch {
            System.Console.WriteLine ();
        }
    }
}");
        }
    }
}

