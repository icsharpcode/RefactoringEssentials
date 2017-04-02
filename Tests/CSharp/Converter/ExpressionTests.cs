﻿using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
	public class ExpressionTests : ConverterTestBase
    {
		[Fact(Skip = "Not implemented!")]
		public void MultilineString()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod()
        Dim x = ""Hello,
World!""
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod()
    {
        var x = @""Hello,
World!"";
    }
}");
        }

        [Fact]
        public void ConditionalExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod(ByVal str As String)
        Dim result As Boolean = If((str = """"), True, False)
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod(string str)
    {
        bool result = (str == """") ? true : false;
    }
}");
        }

        [Fact]
        public void NullCoalescingExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod(ByVal str As String)
        Console.WriteLine(If(str, ""<null>""))
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod(string str)
    {
        Console.WriteLine(str ?? ""<null>"");
    }
}");
        }

        [Fact]
        public void MemberAccessAndInvocationExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod(ByVal str As String)
        Dim length As Integer
        length = str.Length
        Console.WriteLine(""Test"" & length)
        Console.ReadKey()
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod(string str)
    {
        int length;
        length = str.Length;
        Console.WriteLine(""Test"" + length);
        Console.ReadKey();
    }
}");
        }

        [Fact]
        public void ElvisOperatorExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod(ByVal str As String)
        Dim length As Integer = If(str?.Length, -1)
        Console.WriteLine(length)
        Console.ReadKey()
        Dim redirectUri As String = context.OwinContext.Authentication?.AuthenticationResponseChallenge?.Properties?.RedirectUri
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod(string str)
    {
        int length = str?.Length ?? -1;
        Console.WriteLine(length);
        Console.ReadKey();
        string redirectUri = context.OwinContext.Authentication?.AuthenticationResponseChallenge?.Properties?.RedirectUri;
    }
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void ObjectInitializerExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class StudentName
    Public LastName, FirstName As String
End Class

Class TestClass
    Private Sub TestMethod(ByVal str As String)
        Dim student2 As StudentName = New StudentName With {.FirstName = ""Craig"", .LastName = ""Playstead""}
    End Sub
End Class", @"
class StudentName
{
    public string LastName, FirstName;
}

class TestClass
{
    private void TestMethod(string str)
    {
        StudentName student2 = new StudentName
        {
            FirstName = ""Craig"",
            LastName = ""Playstead"",
        };
    }
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void ObjectInitializerExpression2()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod(ByVal str As String)
        Dim student2 = New With {Key .FirstName = ""Craig"", Key .LastName = ""Playstead""}
    End Sub
End Class", @"
class TestClass
{
    private void TestMethod(string str)
    {
        var student2 = new {
            FirstName = ""Craig"",
            LastName = ""Playstead"",
        };
    }
}");
        }

        [Fact]
        public void ThisMemberAccessExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private member As Integer

    Private Sub TestMethod()
        Me.member = 0
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private int member;

    private void TestMethod()
    {
        this.member = 0;
    }
}");
        }

        [Fact]
        public void BaseMemberAccessExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class BaseTestClass
    Public member As Integer
End Class

Class TestClass
    Inherits BaseTestClass

    Private Sub TestMethod()
        MyBase.member = 0
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class BaseTestClass
{
    public int member;
}

class TestClass : BaseTestClass
{
    private void TestMethod()
    {
        base.member = 0;
    }
}");
        }

        [Fact]
        public void DelegateExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod()
        Dim test = Function(ByVal a As Integer) a * 2
        test(3)
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod()
    {
        var test = int a => a * 2;
        test(3);
    }
}");
        }

        [Fact]
        public void LambdaBodyExpression()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod()
        Dim test = Function(a) a * 2
        Dim test2 = Function(a, b)
                        If b > 0 Then Return a / b
                        Return 0
                    End Function

        Dim test3 = Function(a, b) a Mod b
        test(3)
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private void TestMethod()
    {
        var test = a => a * 2;
        var test2 = (a, b) =>
        {
            if (b > 0)
                return a / b;
            return 0;
        };
        var test3 = (a, b) => a % b;
        test(3);
    }
}");
        }

        [Fact]
        public void Await()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Function SomeAsyncMethod() As Task(Of Integer)
        Return Task.FromResult(0)
    End Function

    Private Async Sub TestMethod()
        Dim result As Integer = Await SomeAsyncMethod()
        Console.WriteLine(result)
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    private Task<int> SomeAsyncMethod()
    {
        return Task.FromResult(0);
    }

    private async void TestMethod()
    {
        int result = await SomeAsyncMethod();
        Console.WriteLine(result);
    }
}");
        }

        [Fact(Skip = "Not implemented!")]
        public void Linq1()
        {
            TestConversionVisualBasicToCSharp(@"Private Shared Sub SimpleQuery()
    Dim numbers As Integer() = {7, 9, 5, 3, 6}
    Dim res = From n In numbers Where n > 5 Select n

    For Each n In res
        Console.WriteLine(n)
    Next
End Sub",
				@"static void SimpleQuery()
{
    int[] numbers = { 7, 9, 5, 3, 6 };
 
    var res = from n in numbers
                where n > 5
                select n;
 
    foreach (var n in res)
        Console.WriteLine(n);
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void Linq2()
        {
            TestConversionVisualBasicToCSharp(@"Public Shared Sub Linq40()
    Dim numbers As Integer() = {5, 4, 1, 3, 9, 8, 6, 7, 2, 0}
    Dim numberGroups = From n In numbers Group n By __groupByKey1__ = n Mod 5 Into g Select New With {Key .Remainder = g.Key, Key .Numbers = g}

    For Each g In numberGroups
        Console.WriteLine($""Numbers with a remainder of {g.Remainder} when divided by 5:"")

        For Each n In g.Numbers
            Console.WriteLine(n)
        Next
    Next
End Sub",
				@"public static void Linq40() 
    { 
        int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 }; 
      
        var numberGroups = 
            from n in numbers 
            group n by n % 5 into g 
            select new { Remainder = g.Key, Numbers = g }; 
      
        foreach (var g in numberGroups) 
        { 
            Console.WriteLine($""Numbers with a remainder of {g.Remainder} when divided by 5:""); 
            foreach (var n in g.Numbers)
            {
                Console.WriteLine(n);
            }
        }
    }");
        }

		[Fact(Skip = "Not implemented!")]
		public void Linq3()
        {
            TestConversionVisualBasicToCSharp(@"Class Product
    Public Category As String
    Public ProductName As String
End Class

Class Test
    Public Sub Linq102()
        Dim categories As String() = New String() {""Beverages"", ""Condiments"", ""Vegetables"", ""Dairy Products"", ""Seafood""}
        Dim products As Product() = GetProductList()
        Dim q = From c In categories Join p In products On c Equals p.Category Select New With {Key .Category = c, p.ProductName}

        For Each v In q
            Console.WriteLine($""{v.ProductName}: {v.Category}"")
        Next
    End Sub
End Class",
				@"class Product {
    public string Category;
    public string ProductName;
}

class Test {
    public void Linq102() 
    { 
        string[] categories = new string[]{  
            ""Beverages"",   
            ""Condiments"",
            ""Vegetables"",
            ""Dairy Products"",
            ""Seafood"" };

            Product[] products = GetProductList();

            var q =
                from c in categories
                join p in products on c equals p.Category
                select new { Category = c, p.ProductName }; 
 
        foreach (var v in q) 
        { 
            Console.WriteLine($""{v.ProductName}: {v.Category}"");  
        }
    }
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void Linq4()
        {
            TestConversionVisualBasicToCSharp(@"Public Sub Linq103()
    Dim categories As String() = New String() {""Beverages"", ""Condiments"", ""Vegetables"", ""Dairy Products"", ""Seafood""}
    Dim products = GetProductList()
    Dim q = From c In categories Group Join p In products On c Equals p.Category Into ps = Group Select New With {Key .Category = c, Key .Products = ps}

    For Each v In q
        Console.WriteLine(v.Category & "":"")

        For Each p In v.Products
            Console.WriteLine(""   "" & p.ProductName)
        Next
    Next
End Sub", @"public void Linq103() 
{ 
    string[] categories = new string[]{  
        ""Beverages"",  
        ""Condiments"",
        ""Vegetables"",
        ""Dairy Products"",
        ""Seafood"" };

        var products = GetProductList();

        var q =
            from c in categories
            join p in products on c equals p.Category into ps
            select new { Category = c, Products = ps }; 
  
    foreach (var v in q) 
    { 
        Console.WriteLine(v.Category + "":""); 
        foreach (var p in v.Products) 
        { 
            Console.WriteLine(""   "" + p.ProductName); 
        }
    }
}");
        }
    }
}
