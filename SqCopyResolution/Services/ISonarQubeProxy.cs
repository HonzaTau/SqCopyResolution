using System.Collections.Generic;
using SqCopyResolution.Model.SonarQube;

namespace SqCopyResolution.Services
{
    public interface ISonarQubeProxy
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<Issue> GetIssuesForProject(string projectKey, bool onlyFalsePositivesAndWontFixes);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<Issue> GetAllIssuesForProject(string sourceProjectKey);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<Issue> GetOpenIssuesForProject(string sourceProjectKey);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<Issue> GetFalsePositiveIssuesForProject(string sourceProjectKey);
        void UpdateIssueResolution(string issueKey, string newResolution, Comment[] comments);
    }
}