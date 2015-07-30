Module MainModule

    Sub Main()

        Dim x As Integer = 0
        ' Refactoring to convert "Do ... Loop until" to "Do Until ... Loop" and vice versa
        Do
            x += 1
        Loop Until x = 5

    End Sub

End Module
