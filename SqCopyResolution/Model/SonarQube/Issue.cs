namespace SqCopyResolution.Model.SonarQube
{
    public class Issue
    {
        public string Rule { get; set; }
        public string Component { get; set; }
        public string Resolution { get; set; }
        public string Message { get; set; }
        public TextRange TextRange { get; set; }
    }
}
