using Newtonsoft.Json;

namespace SqCopyResolution.Model.SonarQube
{
    public class ApiIssuesSearchResult
    {
        public Paging Paging { get; set; }
        public Issue[] Issues { get; set; }
    }
}
