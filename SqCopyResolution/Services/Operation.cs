using SqCopyResolution.Model;
using SqCopyResolution.Model.SonarQube;
using System;
using System.Collections.Generic;

namespace SqCopyResolution.Services
{
    public class Operation
    {
        private ILogger Logger { get; set; }
        private ISonarQubeProxy SonarQubeProxy { get; set; }
        private ConfigurationParameters ConfigParameters { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public Operation(ILogger logger, ConfigurationParameters configParameters) : 
            this(logger, configParameters, new SonarQubeProxy(logger, configParameters.SonarQubeUrl, configParameters.UserName, configParameters.Password))
        {}


        public Operation(ILogger logger, ConfigurationParameters configParameters, ISonarQubeProxy sonarQubeProxy)
        {
            Logger = logger;
            ConfigParameters = configParameters;
            SonarQubeProxy = sonarQubeProxy;
        }

        public void Run()
        {
            switch (ConfigParameters.OperationType)
            {
                case OperationType.CopyFalsePositivesAndWontFixes:
                    CopyResolutions();
                    break;
                case OperationType.SyncSubBranches:
                    SyncSubBranches();
                    break;
                default:
                    throw new InvalidOperationException("Unknown operation type: " + ConfigParameters.OperationType);
            }
        }


        private void CopyResolutions()
        {
            Logger.LogInfo("Getting list of issues for project {0}", ConfigParameters.SourceProjectKey);
            var sourceIssues = SonarQubeProxy.GetIssuesForProject(ConfigParameters.SourceProjectKey, true);

            if (sourceIssues.Count > 0)
            {
                foreach (var destinationProjectKey in ConfigParameters.DestinationProjectKeys)
                {
                    Logger.LogInfo("Copying resolutions to project {0}", destinationProjectKey);

                    var destinationIssues = SonarQubeProxy.GetIssuesForProject(destinationProjectKey, false);
                    foreach (var sourceIssue in sourceIssues)
                    {
                        if ((string.CompareOrdinal(sourceIssue.Resolution, "FALSE-POSITIVE") != 0) || (string.CompareOrdinal(sourceIssue.Resolution, "WONTFIX") != 0))
                        {
                            Logger.LogInfo("Issue {0}", sourceIssue);
                            var destinationIssue = FindIssue(destinationIssues, sourceIssue);
                            if (destinationIssue == null)
                            {
                                Logger.LogWarn("\tNot found in the destination project");
                            }
                            else if (!string.IsNullOrEmpty(destinationIssue.Resolution))
                            {
                                Logger.LogInfo("\tIssue is already marked as {0} in the destination project.",
                                    destinationIssue.Resolution);
                            }
                            else
                            {
                                Logger.LogInfo("\tUpdating issue resolution to {0}",
                                    sourceIssue.Resolution);
                                SonarQubeProxy.UpdateIssueResolution(destinationIssue.Key, sourceIssue.Resolution, sourceIssue.Comments);
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogWarn("There are no issues to copy!");
            }
        }

        private void SyncSubBranches()
        {
            CopyFalsePositivesToParentProject();
            MarkAllParentIssuesAsWontFixesInSubBranches();
        }

        private void CopyFalsePositivesToParentProject()
        {
            Logger.LogInfo("Getting list of open issues in parent branch {0}", ConfigParameters.SourceProjectKey);
            var parentIssues = SonarQubeProxy.GetOpenIssuesForProject(ConfigParameters.SourceProjectKey);
            Logger.LogInfo("Found {0} issues", parentIssues.Count);

            Logger.LogInfo("\tGetting list of False-Positive issues in sub-branches");
            foreach (var destinationProjectKey in ConfigParameters.DestinationProjectKeys)
            {
                if (parentIssues.Count > 0)
                {
                    Logger.LogInfo("\t\tGetting list of False-Positive issues for project {0}", destinationProjectKey);
                    var subBranchIssues = SonarQubeProxy.GetFalsePositiveIssuesForProject(destinationProjectKey);
                    Logger.LogInfo("\t\tFound {0} issues", subBranchIssues.Count);
                    if (subBranchIssues.Count > 0)
                    {
                        var numberOfUpdatedIssues = 0;
                        Logger.LogInfo("\t\t\tCopying False-Positive resolutions to parent branch");
                        foreach (var subBranchIssue in subBranchIssues)
                        {
                            Logger.LogDebug("Issue {0}", subBranchIssue);

                            var parentIssue = FindIssue(parentIssues, subBranchIssue);

                            if (parentIssue == null)
                            {
                                Logger.LogDebug("Issue not found in parent branch");
                            }
                            else
                            {
                                Logger.LogDebug("Updating issue resolution to {0} in parent branch",
                                    subBranchIssue.Resolution);
                                SonarQubeProxy.UpdateIssueResolution(parentIssue.Key, subBranchIssue.Resolution, subBranchIssue.Comments);
                                parentIssues.Remove(parentIssue);
                                numberOfUpdatedIssues++;
                            }
                        }
                        Logger.LogInfo("\t\t\tUpdated {0} issues in parent branch", numberOfUpdatedIssues);
                    }
                }
            }
        }

        private void MarkAllParentIssuesAsWontFixesInSubBranches()
        {
            Logger.LogInfo("Getting list of all issues in parent branch {0}", ConfigParameters.SourceProjectKey);
            var parentIssues = SonarQubeProxy.GetAllIssuesForProject(ConfigParameters.SourceProjectKey);
            Logger.LogInfo("Found {0} issues", parentIssues.Count);

            Logger.LogInfo("\tGetting list of open issues in sub-branches");
            foreach (var destinationProjectKey in ConfigParameters.DestinationProjectKeys)
            {
                if (parentIssues.Count > 0)
                {
                    Logger.LogInfo("\t\tGetting list of open issues for project {0}", destinationProjectKey);
                    var subBranchIssues = SonarQubeProxy.GetOpenIssuesForProject(destinationProjectKey);
                    Logger.LogInfo("\t\tFound {0} issues", subBranchIssues.Count);
                    if (subBranchIssues.Count > 0)
                    {
                        var numberOfUpdatedIssues = 0;

                        Logger.LogInfo("\t\t\tCopying resolutions to sub-branch");
                        foreach (var parentIssue in parentIssues)
                        {
                            Logger.LogDebug("Issue {0}", parentIssue);

                            var subBranchIssue = FindIssue(subBranchIssues, parentIssue);

                            if (subBranchIssue == null)
                            {
                                Logger.LogDebug("Issue not found in parent branch");
                            }
                            else if (string.IsNullOrEmpty(parentIssue.Resolution))
                            {
                                Logger.LogDebug("Updating issue resolution to Won't Fix in sub-branch");
                                SonarQubeProxy.UpdateIssueResolution(subBranchIssue.Key,
                                    "WONTFIX",
                                    new[] { new Comment() { HtmlText = "Issue was inherited from the parent stream" } });
                                subBranchIssues.Remove(subBranchIssue);
                                numberOfUpdatedIssues++;
                            }
                            else if (
                                (string.CompareOrdinal(parentIssue.Resolution, "FALSE-POSITIVE") == 0) 
                                || (string.CompareOrdinal(parentIssue.Resolution, "WONTFIX") == 0))
                            {
                                Logger.LogDebug("Updating issue resolution to {0} in sub-branch", parentIssue.Resolution);
                                SonarQubeProxy.UpdateIssueResolution(subBranchIssue.Key, parentIssue.Resolution, parentIssue.Comments);
                                subBranchIssues.Remove(subBranchIssue);
                                numberOfUpdatedIssues++;
                            }
                        }
                        Logger.LogInfo("\t\t\tUpdated {0} issues in sub-branch", numberOfUpdatedIssues);
                    }
                }
            }
        }

        private static Issue FindIssue(List<Issue> issuesList, Issue issue)
        {
            return issuesList.Find((i) =>
                (
                    (i.Message == issue.Message)
                    || 
                    (issue.StartLine == 0 && issue.StartOffset == 0)
                )
                && i.Rule == issue.Rule
                && i.ComponentPath == issue.ComponentPath
                && i.StartLine >= (0.9 * issue.StartLine)
                && i.StartLine <= (1.1 * issue.StartLine)
                && i.StartOffset >= (0.9 * issue.StartOffset)
                && i.StartOffset <= (1.1 * issue.StartOffset));
        }
    }
}
