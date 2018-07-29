Partial Module Program
    Friend Class TestIrisData
        Friend Shared ReadOnly Iris1 As New IrisData With {
            .SepalLength = 5.1F,
            .SepalWidth = 3.3F,
            .PetalLength = 1.6F,
            .PetalWidth = 0.2F
        }
        Friend Shared ReadOnly Iris2 As New IrisData With {
            .SepalLength = 6.4F,
            .SepalWidth = 3.1F,
            .PetalLength = 5.5F,
            .PetalWidth = 2.2F
        }
        Friend Shared ReadOnly Iris3 As New IrisData With {
            .SepalLength = 4.4F,
            .SepalWidth = 3.1F,
            .PetalLength = 2.5F,
            .PetalWidth = 1.2F
        }
    End Class
End Module
