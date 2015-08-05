using NUnit.Framework;
using RefactoringEssentials.VB.CodeRefactorings;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    [TestFixture]
    public class CheckDictionaryKeyValueTests : VBCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        Console.WriteLine(args($5))
    End Sub
End Class", @"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        Dim val As Integer
        If args.TryGetValue(5, val) Then
            Console.WriteLine(val)
        End If
    End Sub
End Class");
        }

        [Test]
        public void TestNestedCase1()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If True Then
            If True Then
                Console.WriteLine(args($5 + 234 - 234))
            End If
        End If
    End Sub
End Class", @"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If True Then
            If True Then
                Dim val As Integer
                If args.TryGetValue(5 + 234 - 234, val) Then
                    Console.WriteLine(val)
                End If
            End If
        End If
    End Sub
End Class");
        }

        [Test]
        public void TestNestedCase2()
        {
            TestWrongContext<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        Dim val As Integer
        If args.TryGetValue($5, val) Then
            Console.WriteLine(val)
        End If
    End Sub
End Class");
        }

        [Test]
        public void TestNameClash()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Private Shared val As Integer

    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        Console.WriteLine(args($5))
    End Sub
End Class", @"
Class Test
    Private Shared val As Integer

    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        Dim val1 As Integer
        If args.TryGetValue(5, val1) Then
            Console.WriteLine(val1)
        End If
    End Sub
End Class");
        }
    }
}