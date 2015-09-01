Public Class SomeClass

    Public Sub New(Name As String)
        If Name Is Nothing Then
            ' Analyzer + CodeFix to replace "Name" by NameOf(Name)
            Throw New ArgumentNullException("Name")
        End If
    End Sub

    Dim _count As Integer
    ' "Add another accessor" refactoring
    Public ReadOnly Property Count As Integer
        Get
            Return _count
        End Get
    End Property

    Dim _name As String
    ' "Create changed event" refactoring
    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property

End Class
