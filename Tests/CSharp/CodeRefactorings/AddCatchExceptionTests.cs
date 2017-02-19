using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
	public class AddCatchExceptionTests : CSharpCodeRefactoringTestBase
    {

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
