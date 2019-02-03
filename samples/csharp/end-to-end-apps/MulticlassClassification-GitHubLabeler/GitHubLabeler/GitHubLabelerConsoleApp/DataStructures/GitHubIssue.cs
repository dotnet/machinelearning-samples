
#pragma warning disable 649 // We don't care about unsused fields here, because they are mapped with the input file.

using Microsoft.ML.Data;

namespace GitHubLabeler.DataStructures
{
    //The only purpose of this class is for peek data after transforming it with the pipeline
    internal class GitHubIssue
    {
        [LoadColumn(0)]
        public string ID;

        [LoadColumn(1)]
        public string Area; // This is an issue label, for example "area-System.Threading"

        [LoadColumn(2)]
        public string Title;

        [LoadColumn(3)]
        public string Description;
    }
}
