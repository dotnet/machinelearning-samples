using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace TitanicSurvivalConsoleApp
{
    public class DataProcessor
    {
        public IEstimator<ITransformer> DataProcessPipeline { get; private set; }

        public DataProcessor(MLContext mlContext)
        {
            // Configure data transformations in the Process pipeline
            // In our case, we will one-hot encode as categorical values 
            // Then concatenate those encoded values into a new "features" column. 
                           
            DataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding("Sex", "SexEncoded")
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Age", "AgeEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Cabin", "CabinEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Pclass", "PclassEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("SibSp", "SibSpEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Parch", "ParchEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Embarked", "EmbarkedEncoded"))                         
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Ticket", "TicketEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Fare", "FareEncoded"))
                          .Append(mlContext.Transforms.Categorical.OneHotEncoding("Cabin", "CabinEncoded"))
                          // Put all features into a vector, including "age" original numeric values (Except the Label ("Survived"), and "Name" and "PassengerId" that won't impact)
                          .Append(mlContext.Transforms.Concatenate("Features", //Output encoded features
                                                                   "PclassEncoded",
                                                                   "SexEncoded",
                                                                   "AgeEncoded",
                                                                   "SibSpEncoded",
                                                                   "ParchEncoded",
                                                                   "TicketEncoded",
                                                                   "FareEncoded",
                                                                   "CabinEncoded",
                                                                   "EmbarkedEncoded"));
        }
    }
}


