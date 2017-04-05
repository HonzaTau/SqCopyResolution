namespace SqCopyResolution.Model.SonarQube
{
    public class ApiIssuesSearchResult
    {
        public Paging Paging { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is used just for passing objects from server")]
        public Issue[] Issues { get; set; }
    }
}
