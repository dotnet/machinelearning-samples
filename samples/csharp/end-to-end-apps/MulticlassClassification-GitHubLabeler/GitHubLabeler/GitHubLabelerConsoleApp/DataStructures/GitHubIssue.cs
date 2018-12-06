using Microsoft.ML.Runtime.Api;

#pragma warning disable 649 // We don't care about unsused fields here, because they are mapped with the input file.

namespace GitHubLabeler.DataStructures
{
    //The only purpose of this class is for peek data after transforming it with the pipeline
    internal class GitHubIssue
    {
        public string ID;

        public string Area; // This is an issue label, for example "area-System.Threading"

        public string Title;

        public string Description;
    }
}
