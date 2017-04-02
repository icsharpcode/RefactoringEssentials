using RefactoringEssentials.VB.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.VB.Diagnostics
{
	public class NameOfSuggestionTests : VBDiagnosticTestBase
    {
        [Fact]
        public void TestArgumentNullExceptionInMethod()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Sub F(foo As Integer)
		Throw New ArgumentNullException($""foo""$, ""bar"")
	End Sub
End Class", @"
Imports System;
Class A
	Sub F(foo As Integer)
		Throw New ArgumentNullException(NameOf(foo), ""bar"")
	End Sub
End Class");
        }

        [Fact]
        public void TestArgumentException()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Sub F(foo As Integer)
		If foo IsNot Nothing
			Throw New ArgumentException(""bar"", $""foo""$)
		End If
	End Sub
End Class", @"
Imports System;
Class A
	Sub F(foo As Integer)
		If foo IsNot Nothing
			Throw New ArgumentException(""bar"", NameOf(foo))
		End If
	End Sub
End Class");
        }

        [Fact]
        public void TestArgumentOutOfRangeExceptionSwap()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Sub F(foo As Integer)
		Throw New ArgumentOutOfRangeException($""foo""$, ""foo"")
	End Sub
End Class", @"
Imports System;
Class A
	Sub F(foo As Integer)
		Throw New ArgumentOutOfRangeException(NameOf(foo), ""foo"")
	End Sub
End Class", 0);
        }

        [Fact]
        public void TestArgumentNullExceptionInConstructor()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Sub New(foo As Integer)
		Throw New ArgumentNullException($""foo""$, ""bar"")
	End Sub
End Class", @"
Imports System;
Class A
	Sub New(foo As Integer)
		Throw New ArgumentNullException(NameOf(foo), ""bar"")
	End Sub
End Class");
        }

        [Fact]
        public void TestArgumentNullExceptionInEventHandlerAccessor()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Public Custom Event Click As EventHandler
		AddHandler(ByVal value As EventHandler)
			Throw New ArgumentNullException($""value""$, ""bar"")
		End AddHandler
		RemoveHandler(ByVal value As EventHandler)
			Throw New ArgumentNullException($""value""$, ""bar"")
		End RemoveHandler
	End Event
End Class", @"
Imports System;
Class A
	Public Custom Event Click As EventHandler
		AddHandler(ByVal value As EventHandler)
			Throw New ArgumentNullException(NameOf(value), ""bar"")
		End AddHandler
		RemoveHandler(ByVal value As EventHandler)
			Throw New ArgumentNullException(NameOf(value), ""bar"")
		End RemoveHandler
	End Event
End Class");
        }

        [Fact]
        public void TestArgumentNullExceptionInGetAccessor()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Property P As String
		Get
		End Get
		Set(value As String)
			Throw New ArgumentNullException($""value""$, ""bar"")
		End Set
	End Property
End Class", @"
Imports System;
Class A
	Property P As String
		Get
		End Get
		Set(value As String)
			Throw New ArgumentNullException(NameOf(value), ""bar"")
		End Set
	End Property
End Class");
        }

        [Fact]
        public void TestArgumentNullExceptionInIndexer()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Default Property Item(ByVal i As Integer) As String
		Get
			Throw New ArgumentNullException($""i""$, ""bar"")
		End Get
		Set(value As String)
		End Set
	End Property
End Class", @"
Imports System;
Class A
	Default Property Item(ByVal i As Integer) As String
		Get
			Throw New ArgumentNullException(NameOf(i), ""bar"")
		End Get
		Set(value As String)
		End Set
	End Property
End Class");
        }

        [Fact]
        public void TestArgumentNullExceptionInLambda()
        {
            Analyze<NameOfSuggestionAnalyzer>(@"
Imports System;
Class A
	Sub F()
		Dim func = Function(i)
						Throw New ArgumentNullException($""i""$, ""bar"")
				   End Function
	End Sub
End Class", @"
Imports System;
Class A
	Sub F()
		Dim func = Function(i)
						Throw New ArgumentNullException(NameOf(i), ""bar"")
				   End Function
	End Sub
End Class");
        }
    }
}

