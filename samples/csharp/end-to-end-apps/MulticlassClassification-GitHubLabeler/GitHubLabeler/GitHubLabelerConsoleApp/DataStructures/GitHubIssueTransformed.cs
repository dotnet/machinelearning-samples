
#pragma warning disable 649 // We don't care about unused fields here, because they are mapped with the input file.

namespace GitHubLabeler.DataStructures
{
    internal class GitHubIssueTransformed
    {
        public string ID;
        public string Area;
        //public float[] Label;                 // -> Area dictionarized
        public string Title;
        //public float[] TitleFeaturized;       // -> Title Featurized 
        public string Description;
        //public float[] DescriptionFeaturized; // -> Description Featurized 
    }
}


//public Scalar<bool> label { get; set; }
//public Scalar<float> score { get; set; }
