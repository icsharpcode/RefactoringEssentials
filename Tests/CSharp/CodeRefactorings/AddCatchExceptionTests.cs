using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class AddCatchExceptionTests : CSharpCodeRefactoringTestBase
    {

        [Test]
        public void HandlesBasicCase()
        {
            Test<AddCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try {
        }
        catch$ {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try {
        }
        catch (System.Exception e)
        {
        }
    }
}");
        }

        [Test]
        public void HandlesBasicCaseWithBraceOnOwnLine()
        {
            Test<AddCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try
        {
        }
        catch$
        {
        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try
        {
        }
        catch (System.Exception e)
        {
        }
    }
}");
        }

        [Test]
        public void PreservesWhitespaceInBody()
        {
            Test<AddCatchExceptionCodeRefactoringProvider>(@"
class TestClass
{
    public void F()
    {
        try {
        }
        catch$ {

        }
    }
}", @"
class TestClass
{
    public void F()
    {
        try {
        }
        catch (System.Exception e)
        {

        }
    }
}");
        }

        [Test]
        public void DoesNotUseRedundantNamespace()
        {
            Test<AddCatchExceptionCodeRefactoringProvider>(@"
using System;
class TestClass
{
    public void F()
    {
        try {
        }
        catch$ {
        }
    }
}", @"
using System;
class TestClass
{
    public void F()
    {
        try {
        }
        catch (Exception e)
        {
        }
    }
}");
        }

        [Test]
        public void DoesNotMatchCatchesWithType()
        {
            TestWrongContext<AddCatchExceptionCodeRefactoringProvider>(@"
using System;
class TestClass
{
    public void F()
    {
        try {
        }
        catch$ (Exception) {
        }
    }
}");
        }
    }
}
