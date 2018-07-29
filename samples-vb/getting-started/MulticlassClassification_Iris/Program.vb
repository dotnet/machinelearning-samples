Imports System.IO
Imports Microsoft.ML
Imports Microsoft.ML.Data
Imports Microsoft.ML.Models
Imports Microsoft.ML.Trainers
Imports Microsoft.ML.Transforms

Partial Module Program
    Private ReadOnly Property AppPath As String
        Get
            Return Path.GetDirectoryName(Environment.GetCommandLineArgs()(0))
        End Get
    End Property

    Private ReadOnly Property TrainDataPath As String
        Get
            Return Path.Combine(AppPath, "datasets", "iris-train.txt")
        End Get
    End Property

    Private ReadOnly Property TestDataPath As String
        Get
            Return Path.Combine(AppPath, "datasets", "iris-test.txt")
        End Get
    End Property

    Private ReadOnly Property ModelPath As String
        Get
            Return Path.Combine(AppPath, "IrisModel.zip")
        End Get
    End Property

    Sub Main(args As String())
        MainAsync(args).Wait()
    End Sub

    Private Async Function MainAsync(args As String()) As Task
        ' STEP 1: Create a model
        Dim model = Await TrainAsync()
        ' STEP2: Test accuracy
        Evaluate(model)
        ' STEP 3: Make a prediction
        Console.WriteLine()
        Dim prediction = model.Predict(TestIrisData.Iris1)
        Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {prediction.Score(0):0.####}")
        Console.WriteLine($"                                           versicolor:  {prediction.Score(1):0.####}")
        Console.WriteLine($"                                           virginica:   {prediction.Score(2):0.####}")
        Console.WriteLine()
        prediction = model.Predict(TestIrisData.Iris2)
        Console.WriteLine($"Actual: virginica.  Predicted probability: setosa:      {prediction.Score(0):0.####}")
        Console.WriteLine($"                                           versicolor:  {prediction.Score(1):0.####}")
        Console.WriteLine($"                                           virginica:   {prediction.Score(2):0.####}")
        Console.WriteLine()
        prediction = model.Predict(TestIrisData.Iris3)
        Console.WriteLine($"Actual: versicolor. Predicted probability: setosa:      {prediction.Score(0):0.####}")
        Console.WriteLine($"                                           versicolor:  {prediction.Score(1):0.####}")
        Console.WriteLine($"                                           virginica:   {prediction.Score(2):0.####}")
        Console.ReadLine()
    End Function

    Friend Async Function TrainAsync() As Task(Of PredictionModel(Of IrisData, IrisPrediction))
        ' LearningPipeline holds all steps of the learning process: data, transforms, learners.
        ' The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
        ' all the column names and their types.
        ' When ML model starts training, it looks for two columns: Label and Features.
        ' Transforms
        '              like in this example, no extra actions required.
        ' Label:   values that should be predicted. If you have a field named Label in your data type,
        '          If you don’t have it, copy the column you want to predict with ColumnCopier transform:
        '              new ColumnCopier(("FareAmount", "Label"))
        ' Features: all data used for prediction. At the end of all transforms you need to concatenate
        '              all columns except the one you want to predict into Features column with
        '              ColumnConcatenator transform:

        ' StochasticDualCoordinateAscentClassifier is an algorithm that will be used to train the model.
        Dim pipeline As New LearningPipeline From {
            New TextLoader(TrainDataPath).CreateFrom(Of IrisData)(),
            New ColumnConcatenator("Features", "SepalLength", "SepalWidth", "PetalLength", "PetalWidth"),
            New StochasticDualCoordinateAscentClassifier()
        }
        Console.WriteLine("=============== Training model ===============")
        ' The pipeline is trained on the dataset that has been loaded and transformed.
        Dim model = pipeline.Train(Of IrisData, IrisPrediction)()
        ' Saving the model as a .zip file.
        Await model.WriteAsync(ModelPath)
        Console.WriteLine("=============== End training ===============")
        Console.WriteLine("The model is saved to {0}", ModelPath)
        Return model
    End Function

    Private Sub Evaluate(model As PredictionModel(Of IrisData, IrisPrediction))
        ' To evaluate how good the model predicts values, the model Is ran against New set
        ' of data (test data) that was Not involved in training.
        Dim testData = New TextLoader(TestDataPath).CreateFrom(Of IrisData)()
        ' ClassificationEvaluator performs evaluation for Multiclass Classification type of ML problems.
        Dim evaluator As New ClassificationEvaluator With {
            .OutputTopKAcc = 3
        }
        Console.WriteLine("=============== Evaluating model ===============")
        Dim metrics = evaluator.Evaluate(model, testData)
        Console.WriteLine("Metrics:")
        Console.WriteLine($"    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better")
        Console.WriteLine($"    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better")
        Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better")
        Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss(0):0.####}, the closer to 0, the better")
        Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss(1):0.####}, the closer to 0, the better")
        Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss(2):0.####}, the closer to 0, the better")
        Console.WriteLine()
        Console.WriteLine($"    ConfusionMatrix:")

        ' Print confusion matrix
        For i = 0 To metrics.ConfusionMatrix.Order - 1
            For j = 0 To metrics.ConfusionMatrix.ClassNames.Count - 1
                Console.Write(vbTab & metrics.ConfusionMatrix(i, j))
            Next
            Console.WriteLine()
        Next

        Console.WriteLine("=============== End evaluating ===============")
        Console.WriteLine()
    End Sub
End Module
