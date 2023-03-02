Imports System.IO

Public Class Float
    Public Property Buffer As Byte()
    Public Property Loss As Double
    Public Property [Error] As Double
    Public Property Normalized As Double
    Public Property Precision As Precision
    Public Property Time As TimeSpan
    Private Property Clock As Stopwatch

    Sub New(value As Double)
        Me.Clock = New Stopwatch
        Me.Precision = Precision.Double
        Me.Clock.Start()
        Me.Buffer = Me.Pack(Me.ToBinary(value))
        Me.Initialize(value)
        Me.Clock.Stop()
        Me.Time = Me.Clock.Elapsed
    End Sub

    Sub New(value As Double, precision As Precision)
        Me.Clock = New Stopwatch
        Me.Precision = precision
        Me.Clock.Start()
        Me.Buffer = Me.Pack(Me.ToBinary(value))
        Me.Initialize(value)
        Me.Clock.Stop()
        Me.Time = Me.Clock.Elapsed
    End Sub

    Public Function ToDouble() As Double
        Return Me.ToFloat(Me.Unpack(Me.Buffer))
    End Function

    Public Sub Save(filename As String, Optional Overwrite As Boolean = False)
        If (File.Exists(filename) AndAlso Overwrite) Then File.Delete(filename)
        Using bw As New BinaryWriter(File.Open(filename, FileMode.OpenOrCreate))
            Me.Buffer = New Byte(CInt(bw.BaseStream.Length)) {}
            bw.Write(Me.Buffer)
        End Using
    End Sub

    Public Sub Open(filename As String)
        If (Not File.Exists(filename)) Then Throw New FileNotFoundException(filename)
        Using bw As New BinaryReader(File.Open(filename, FileMode.Open))
            Me.Buffer = bw.ReadBytes(CInt(bw.BaseStream.Length))
        End Using
    End Sub

    Public Overrides Function ToString() As String
        Return Me.ToDouble.ToString
    End Function

    ''' <summary>
    ''' Calculates the loss, error and normalized accuracy vector
    ''' </summary>
    Private Sub Initialize(value As Double)
        Me.Loss = Math.Pow(2, 64 - Me.ToPrecision)
        Me.Error = Math.Abs(value - CDbl(value) * Me.Loss)
        Me.Normalized = Me.Error / value
    End Sub

    ''' <summary>
    ''' Assigns the precision
    ''' </summary>
    Private ReadOnly Property ToPrecision As Integer
        Get
            Select Case Me.Precision
                Case Precision.Nibble : Return 4
                Case Precision.Byte : Return 8
                Case Precision.Half : Return 16
                Case Precision.Single : Return 32
                Case Precision.Double : Return 64
            End Select
            Throw New Exception("undefined precision unit")
        End Get
    End Property

    ''' <summary>
    ''' Converts a double to the binary sequence
    ''' </summary>
    Private Function ToBinary(value As Double) As List(Of Integer)
        Dim truncated As Double = Math.Truncate(Math.Abs(value))
        If (truncated > Int64.MaxValue) Then Throw New Exception("number too large")
        Dim sign As Integer = If(value >= 0, 0, 1)
        Dim int As Int64 = Convert.ToInt64(truncated)
        Dim dec As Double = Math.Abs(value) - int
        Dim output As New List(Of Integer)
        output.Add(sign)
        output.AddRange(Me.GetSequence(int))
        output.AddRange(Me.GetSequence(dec))
        Return output
    End Function

    ''' <summary>
    ''' Converts a sequence to a double
    ''' </summary>
    Private Function ToFloat(values As List(Of Integer)) As Double
        Dim sign As Integer = values.First
        Dim int As Long = Me.GetInt32(values.GetRange(1, Me.ToPrecision))
        Dim dec As Double = Me.GetDouble(values.GetRange(Me.ToPrecision + 1, Me.ToPrecision))
        Dim output As Double = int + dec
        If sign = 1 Then output *= -1
        Return output
    End Function

    ''' <summary>
    ''' Converts a int64 to the binary sequence
    ''' </summary>
    Private Function GetSequence(value As Long) As List(Of Integer)
        Dim output As New List(Of Integer)
        While value <> 0
            Dim remainder As Integer = value Mod 2
            output.Insert(0, remainder)
            value \= 2
        End While
        While output.Count < Me.ToPrecision
            output.Insert(0, 0)
        End While
        Return output
    End Function

    ''' <summary>
    ''' Converts a double to the binary sequence
    ''' </summary>
    Private Function GetSequence(value As Double) As List(Of Integer)
        Dim part As Integer = 0
        Dim count As Integer = 0
        Dim output As New List(Of Integer)
        While value <> 0 And count < Me.ToPrecision
            value *= 2
            part = Math.Truncate(value)
            output.Add(part)
            value -= part
            count += 1
        End While
        While output.Count < Me.ToPrecision
            output.Add(0)
        End While
        Return output
    End Function

    ''' <summary>
    ''' Converts a sequence of binary to integer
    ''' </summary>
    Private Function GetInt32(values As List(Of Integer)) As Integer
        Dim mask As Integer = 1
        Dim result As Integer = 0
        For i As Integer = values.Count - 1 To 0 Step -1
            If values(i) = 1 Then result = result Or mask
            mask <<= 1
        Next
        Return result
    End Function

    ''' <summary>
    ''' Converts a sequence of binary to double
    ''' </summary>
    Private Function GetDouble(values As List(Of Integer)) As Double
        Dim result As Double = 0
        Dim factor As Double = 0.5
        For i As Integer = 0 To values.Count - 1
            If values(i) = 1 Then result += factor
            factor /= 2
        Next
        Return result
    End Function

    ''' <summary>
    ''' Packs a sequence of intgers into a byte array
    ''' </summary>
    Private Function Pack(value As List(Of Integer)) As Byte()
        Dim total As Integer = value.Count
        Dim count As Integer = Math.Ceiling(total / 8)
        Dim padding As Integer = count * 8 - total
        Dim bytes(count) As Byte
        Dim byteIndex As Integer = 0
        Dim bitIndex As Integer = 0
        For i As Integer = 0 To total - 1
            If value(i) = 1 Then
                bytes(byteIndex) = bytes(byteIndex) Or CByte(1 << bitIndex)
            End If
            bitIndex += 1
            If bitIndex = 8 Then
                bitIndex = 0
                byteIndex += 1
            End If
        Next
        bytes(count) = padding
        Return bytes
    End Function

    ''' <summary>
    ''' Unpacks a sequence of intgers into a byte array
    ''' </summary>
    Private Function Unpack(values As Byte()) As List(Of Integer)
        Dim bits As New List(Of Integer)
        Dim padding As Integer = values(values.Length - 1)
        Dim count As Integer = (values.Length - 1) * 8 - padding
        For i As Integer = 0 To count - 1
            Dim byteIndex As Integer = i \ 8
            Dim bitIndex As Integer = i Mod 8
            If (values(byteIndex) And (1 << bitIndex)) <> 0 Then
                bits.Add(1)
            Else
                bits.Add(0)
            End If
        Next
        Return bits
    End Function

End Class
