using System;
using Microsoft.ML.Transforms;

namespace SpamDetectionConsoleApp
{
    public class CustomMappings : CustomMappingFactory<InputRow, OutputRow>
    {
        public static void IncomeMapping(InputRow input, OutputRow output) => output.Label = input.Label == "spam" ? true : false;

        public override Action<InputRow, OutputRow> GetMapping()
        {
            return IncomeMapping;
        }
    }

    public class InputRow
    {
        public string Label { get; set; }
    }

    public class OutputRow
    {
        public bool Label { get; set; }
    }
}
