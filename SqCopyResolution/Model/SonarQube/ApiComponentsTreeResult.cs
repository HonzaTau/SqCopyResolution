namespace SqCopyResolution.Model.SonarQube
{
    public class ApiComponentsTreeResult
    {
        public Paging Paging { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is used just for passing objects from server")]
        public Component[] Components { get; set; }
    }
}
