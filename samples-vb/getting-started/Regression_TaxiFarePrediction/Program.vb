Imports System.IO
Imports Microsoft.ML
Imports Microsoft.ML.Data
Imports Microsoft.ML.Models
Imports Microsoft.ML.Trainers
Imports Microsoft.ML.Transforms

Friend Module Program
    Private ReadOnly Property AppPath As String
        Get
            Return Path.GetDirectoryName(Environment.GetCommandLineArgs()(0))
        End Get
    End Property

    Private ReadOnly Property TrainDataPath As String
        Get
            Return Path.Combine(AppPath, "datasets", "taxi-fare-train.csv")
        End Get
    End Property

    Private ReadOnly Property TestDataPath As String
        Get
            Return Path.Combine(AppPath, "datasets", "taxi-fare-test.csv")
        End Get
    End Property

    Private ReadOnly Property ModelPath As String
        Get
            Return Path.Combine(AppPath, "TaxiFareModel.zip")
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
        Dim prediction = model.Predict(TestTaxiTrips.Trip1)
        Console.WriteLine($"Predicted fare: {prediction.FareAmount:0.####}, actual fare: 29.5")
        Console.ReadLine()
    End Function

    Private Async Function TrainAsync() As Task(Of PredictionModel(Of TaxiTrip, TaxiTripFarePrediction))
        ' LearningPipeline holds all steps of the learning process: data, transforms, learners.
        ' The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
        ' all the column names and their types.
        ' Transforms
        ' When ML model starts training, it looks for two columns: Label and Features.
        ' Label:   values that should be predicted. If you have a field named Label in your data type,
        '              no extra actions required.
        '          If you don't have it, like in this example, copy the column you want to predict with
        '              ColumnCopier transform:
        ' CategoricalOneHotVectorizer transforms categorical (string) values into 0/1 vectors
        ' Features: all data used for prediction. At the end of all transforms you need to concatenate
        '              all columns except the one you want to predict into Features column with
        '              ColumnConcatenator transform:
        ' FastTreeRegressor is an algorithm that will be used to train the model.
        Dim pipeline As New LearningPipeline From {
            New TextLoader(TrainDataPath).CreateFrom(Of TaxiTrip)(separator:=","c),
            New ColumnCopier(("FareAmount", "Label")),
            New CategoricalOneHotVectorizer("VendorId",
                "RateCode",
                "PaymentType"),
            New ColumnConcatenator("Features",
                "VendorId",
                "RateCode",
                "PassengerCount",
                "TripDistance",
                "PaymentType"),
            New FastTreeRegressor()
        }

        Console.WriteLine("=============== Training model ===============")
        ' The pipeline is trained on the dataset that has been loaded and transformed.
        Dim model = pipeline.Train(Of TaxiTrip, TaxiTripFarePrediction)()
        ' Saving the model as a .zip file.
        Await model.WriteAsync(ModelPath)
        Console.WriteLine("=============== End training ===============")
        Console.WriteLine("The model is saved to {0}", ModelPath)
        Return model
    End Function

    Private Sub Evaluate(model As PredictionModel(Of TaxiTrip, TaxiTripFarePrediction))
        ' To evaluate how good the model predicts values, it Is run against New set
        ' of data (test data) that was Not involved in training.
        Dim testData = New TextLoader(TestDataPath).CreateFrom(Of TaxiTrip)(separator:=","c)
        ' RegressionEvaluator calculates the differences (in various metrics) between predicted And actual
        ' values in the test dataset.
        Dim evaluator As New RegressionEvaluator
        Console.WriteLine("=============== Evaluating model ===============")
        Dim metrics = evaluator.Evaluate(model, testData)
        Console.WriteLine($"Rms = {metrics.Rms}, ideally should be around 2.8, can be improved with larger dataset")
        Console.WriteLine($"RSquared = {metrics.RSquared}, a value between 0 and 1, the closer to 1, the better")
        Console.WriteLine("=============== End evaluating ===============")
        Console.WriteLine()
    End Sub
End Module
