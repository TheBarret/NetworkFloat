Imports System.IO
Imports System.Security.Cryptography

Public MustInherit Class ByteArray

    Public Property Buffer As Byte()

    Public MustOverride Function ToValue(Of T)() As T

    Sub New(value As Byte())
        Me.Buffer = value
    End Sub


End Class
