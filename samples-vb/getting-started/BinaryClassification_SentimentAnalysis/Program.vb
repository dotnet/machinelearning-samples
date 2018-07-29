Imports System.IO
Imports Microsoft.ML
Imports Microsoft.ML.Data
Imports Microsoft.ML.Models
Imports Microsoft.ML.Runtime.Api
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
            Return Path.Combine(AppPath, "datasets", "sentiment-imdb-train.txt")
        End Get
    End Property

    Private ReadOnly Property TestDataPath As String
        Get
            Return Path.Combine(AppPath, "datasets", "sentiment-yelp-test.txt")
        End Get
    End Property

    Private ReadOnly Property ModelPath As String
        Get
            Return Path.Combine(AppPath, "SentimentModel.zip")
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
        Dim predictions = model.Predict(TestSentimentData.Sentiments)

        Dim sentimentsAndPredictions =
            TestSentimentData.Sentiments.Zip(predictions, Function(sentiment, prediction) (sentiment, prediction))

        For Each item In sentimentsAndPredictions
            Console.WriteLine($"Sentiment: {item.sentiment.SentimentText} | Prediction: {(If(item.prediction.Sentiment, "Positive", "Negative"))} sentiment")
        Next

        Console.ReadLine()
    End Function

    Public Async Function TrainAsync() As Task(Of PredictionModel(Of SentimentData, SentimentPrediction))
        ' LearningPipeline holds all steps of the learning process: data, transforms, learners. 
        ' The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
        ' all the column names and their types.
        ' TextFeaturizer is a transform that will be used to featurize an input column to format and clean the data.
        ' FastTreeBinaryClassifier is an algorithm that will be used to train the model.
        ' It has three hyperparameters for tuning decision tree performance. 
        Dim pipeline As New LearningPipeline From {
            New TextLoader(TrainDataPath).CreateFrom(Of SentimentData)(),
            New TextFeaturizer("Features", "SentimentText"),
            New FastTreeBinaryClassifier() With {
                .NumLeaves = 5,
                .NumTrees = 5,
                .MinDocumentsInLeafs = 2
            }
        }
        Console.WriteLine("=============== Training model ===============")
        ' The pipeline is trained on the dataset that has been loaded and transformed.
        Dim model = pipeline.Train(Of SentimentData, SentimentPrediction)()
        ' Saving the model as a .zip file.
        Await model.WriteAsync(ModelPath)
        Console.WriteLine("=============== End training ===============")
        Console.WriteLine("The model is saved to {0}", ModelPath)
        Return model
    End Function

    Private Sub Evaluate(model As PredictionModel(Of SentimentData, SentimentPrediction))
        ' To evaluate how good the model predicts values, the model Is ran against New set
        ' of data (test data) that was Not involved in training.
        Dim testData = New TextLoader(TestDataPath).CreateFrom(Of SentimentData)()
        ' BinaryClassificationEvaluator performs evaluation for Binary Classification type of ML problems.
        Dim evaluator = New BinaryClassificationEvaluator()
        Console.WriteLine("=============== Evaluating model ===============")
        Dim metrics = evaluator.Evaluate(model, testData)
        ' BinaryClassificationMetrics contains the overall metrics computed by binary classification evaluators
        '  The Accuracy metric gets the accuracy of a classifier which Is the proportion 
        ' of correct predictions in the test set.

        '  The Auc metric gets the area under the ROC curve.
        '  The area under the ROC curve Is equal to the probability that the classifier ranks
        '  a randomly chosen positive instance higher than a randomly chosen negative one
        '  (assuming 'positive' ranks higher than 'negative').

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
