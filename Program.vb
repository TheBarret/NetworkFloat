Module Program

    Sub Main()
        Console.Title = "Custom Float Class Performance Test"
        Do
            Console.Clear()
            Dim sum As Double = New Random().NextDouble
            Dim values As New List(Of Float)
            values.Add(New Float(sum, Precision.Double))
            values.Add(New Float(sum, Precision.Single))
            values.Add(New Float(sum, Precision.Half))
            values.Add(New Float(sum, Precision.Byte))
            values.Add(New Float(sum, Precision.Nibble))
            For Each value As Float In values
                Console.WriteLine("----------------------------------------------------------")
                Console.WriteLine("Input        : {0}", sum)
                Console.WriteLine("Output       : {0}", value.ToDouble)
                Console.WriteLine("Bandwidth    : {0}bits", CInt(value.Precision))
                Console.WriteLine("Time         : {0}", value.Time.Duration)
            Next
            Console.Read()
        Loop
    End Sub
End Module
