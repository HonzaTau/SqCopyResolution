using System.Globalization;

namespace SqCopyResolution.Model.SonarQube
{
    public class Issue
    {
        public string Key { get; set; }
        public string Rule { get; set; }
        public string Component { get; set; }
        public string Project { get; set; }
        public string Resolution { get; set; }
        public string Message { get; set; }
        public TextRange TextRange { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is used just for passing objects from server")]
        public Comment[] Comments { get; set; }

        public string ComponentPath
        {
            get
            {
                return Component.Substring(Project.Length + 1);
            }
        }

        public int StartLine {
            get
            {
                return TextRange == null ? 0 : TextRange.StartLine;
            }
        }

        public int StartOffset
        {
            get
            {
                return TextRange == null ? 0 : TextRange.StartOffset;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "\"{0}\" in component {1}, line {2}", 
                Message, 
                ComponentPath, 
                StartLine);
        }
    }
}
