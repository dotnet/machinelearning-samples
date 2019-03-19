using System.Collections;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HelloML : MonoBehaviour
{
    // ML Model checked in for this app was trained with minimal data for Toxic sentiment analysis, use the following for examples:
    // e.g. of Toxic Sentiment: You are being very rude
    // e.g. of Non Toxic Sentiment: See the section below about the Macedonian last names
    public InputField SentimentField;
    public Text OutputSentiment; 


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Unity ML.NET HelloWorld");
    }

    public void PredictSentiment()
    {
        Loadmlnetmodel();
    }

    public void QuitApplication()
    {
        Debug.Log("Exiting Application");
        Application.Quit();
    }

    void Loadmlnetmodel()
    {
        Debug.Log("Creating Context Object");
        var ctx = new MLContext();
        

        ITransformer loadedModel;
        using (var stream = new FileStream(".\\Assets\\Models\\model.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            loadedModel = ctx.Model.Load(stream);
        }
        Debug.Log("Model Loaded");


        var SentimentText = SentimentField.text;

        ArrayDataViewBuilder arrayDataViewBuilder = new ArrayDataViewBuilder(ctx);
        arrayDataViewBuilder.AddColumn("Text", SentimentText);
        var testissues = arrayDataViewBuilder.GetDataView(rowCount: 1);

        var transformedIssues = loadedModel.Transform(testissues);

        var cursor = transformedIssues.GetRowCursor((i) => true);
        ValueGetter<bool> results = cursor.GetGetter<bool>(transformedIssues.Schema.GetColumnOrNull("PredictedLabel").Value.Index);

        while (cursor.MoveNext())
        {
            bool result = false;
            results(ref result);
            OutputSentiment.text = "Predicted Label for (" + SentimentText + ") is: " + result;
            Debug.Log(OutputSentiment.text);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }


    public class SentimentPrediction
    {
        public bool PredictedLabel { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }

    public class SentimentIssue
    {
        public bool Label { get; set; }

        public string Text { get; set; }
    }
}
