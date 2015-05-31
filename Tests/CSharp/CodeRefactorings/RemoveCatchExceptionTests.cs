using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class RemoveCatchExceptionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

