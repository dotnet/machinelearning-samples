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
            Return Path.Combine(AppPath, "datasets", "titanic-train.csv")
        End Get
    End Property

    Private ReadOnly Property TestDataPath As String
        Get
            Return Path.Combine(AppPath, "datasets", "titanic-test.csv")
        End Get
    End Property

    Private ReadOnly Property ModelPath As String
        Get
            Return Path.Combine(AppPath, "TitanicModel.zip")
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
        Dim prediction = model.Predict(TestTitanicData.Passenger)
        Console.WriteLine($"Did this passenger survive?   Actual: Yes   Predicted: {(If(prediction.Survived, "Yes", "No"))}")
        Console.ReadLine()
    End Function

    Public Async Function TrainAsync() As Task(Of PredictionModel(Of TitanicData, TitanicPrediction))
        ' LearningPipeline holds all steps of the learning process: data, transforms, learners.  
        ' The TextLoader loads a dataset. The schema of the dataset Is specified by passing a class containing
        ' all the column names And their types.
        ' Transform any text feature to numeric values
        ' Put all features into a vector
        ' FastTreeBinaryClassifier Is an algorithm that will be used to train the model.
        ' It has three hyperparameters for tuning decision tree performance. 
        Dim pipeline As New LearningPipeline From {
            New TextLoader(TrainDataPath).CreateFrom(Of TitanicData)(useHeader:=True, separator:=","c),
            New CategoricalOneHotVectorizer("Sex", "Ticket", "Fare", "Cabin", "Embarked"),
            New ColumnConcatenator("Features", "Pclass", "Sex", "Age", "SibSp", "Parch", "Ticket", "Fare", "Cabin", "Embarked"),
            New FastTreeBinaryClassifier With {
                .NumLeaves = 5,
                .NumTrees = 5,
                .MinDocumentsInLeafs = 2
            }
        }
        Console.WriteLine("=============== Training model ===============")
        ' The pipeline Is trained on the dataset that has been loaded And transformed.
        Dim model = pipeline.Train(Of TitanicData, TitanicPrediction)()
        ' Saving the model as a .zip file.
        Await model.WriteAsync(ModelPath)
        Console.WriteLine("=============== End training ===============")
        Console.WriteLine("The model is saved to {0}", ModelPath)
        Return model
    End Function

    Private Sub Evaluate(model As PredictionModel(Of TitanicData, TitanicPrediction))
        ' To evaluate how good the model predicts values, the model Is ran against New set
        ' of data (test data) that was Not involved in training.
        Dim testData = New TextLoader(TestDataPath).CreateFrom(Of TitanicData)(useHeader:=True, separator:=","c)
        ' BinaryClassificationEvaluator performs evaluation for Binary Classification type of ML problems.
        Dim evaluator = New BinaryClassificationEvaluator()
        Console.WriteLine("=============== Evaluating model ===============")
        Dim metrics = evaluator.Evaluate(model, testData)
        ' BinaryClassificationMetrics contains the overall metrics computed by binary classification evaluators
        '  The Accuracy metric gets the accuracy of a classifier which Is the proportion 
        ' of correct predictions in the test set.
        ' 
        '  The Auc metric gets the area under the ROC curve.
        '  The area under the ROC curve Is equal to the probability that the classifier ranks
        '  a randomly chosen positive instance higher than a randomly chosen negative one
        '  (assuming 'positive' ranks higher than 'negative').
        ' 
        '  The F1Score metric gets the classifier's F1 score.
        '  The F1 score Is the harmonic mean of precision And recall:
        '   2 * precision * recall / (precision + recall).

        Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}")
        Console.WriteLine($"Auc: {metrics.Auc:P2}")
        Console.WriteLine($"F1Score: {metrics.F1Score:P2}")
        Console.WriteLine("=============== End evaluating ===============")
        Console.WriteLine()
    End Sub
End Module
