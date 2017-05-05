using Newtonsoft.Json;
using SqCopyResolution.Model.SonarQube;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace SqCopyResolution.Services
{
    public class SonarQubeProxy : ISonarQubeProxy
    {
        private ILogger Logger { get; set; }
        private string SonarQubeUrl { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }

        public SonarQubeProxy(ILogger logger, string sonarQubeUrl, string userName, string password)
        {
            Logger = logger;
            SonarQubeUrl = sonarQubeUrl;
            UserName = userName;
            Password = password;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "I need to call Find method on the returned List, so I cannot use IList here.")]
        public List<Issue> GetIssuesForProject(string projectKey, bool onlyFalsePositivesAndWontFixes)
        {
            return GetIssuesForProject(projectKey: projectKey, resolutions: "FALSE-POSITIVE,WONTFIX");
        }

        private List<Issue> GetIssuesForProject(string projectKey = null, string resolutions = null, string statuses = null)
        {
            var result = new List<Issue>();

            Logger.LogDebug("Getting list of issues for project {0}", projectKey);

            // SonarQube cannot return more than 10000 issues in one response.
            // Let's try to find out, what the number of issues is
            var numberOfIssues = GetNumberOfIssuesForProject(projectKey, resolutions, statuses);

            if (numberOfIssues > 0)
            {
                if (numberOfIssues < 10000)
                {
                    const int pageSize = 500;
                    var pageIndex = 1;
                    do
                    {
                        var uri = GetIssuesSearchUri(pageIndex, pageSize, projectKey: projectKey, resolutions: resolutions, statuses: statuses);

                        var responseContent = GetFromServer(uri);
                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            ApiIssuesSearchResult apiResult = JsonConvert.DeserializeObject<ApiIssuesSearchResult>(responseContent);
                            result.AddRange(apiResult.Issues);
                            if (apiResult.Paging.Total < (apiResult.Paging.PageIndex * apiResult.Paging.PageSize))
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                        pageIndex++;
                    } while (true);
                }
                else
                {
                    // If the number of issues is too high, we need to get their list component by component
                    var components = GetProjectComponents(projectKey);
                    if (components != null)
                    {
                        foreach (var component in components)
                        {
                            result.AddRange(GetIssuesForComponent(component, resolutions, statuses));
                        }
                    }
                }
            }

            Logger.LogDebug("\t{0} issues found", result.Count);

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<Issue> GetAllIssuesForProject(string sourceProjectKey)
        {
            return GetIssuesForProject(projectKey: sourceProjectKey);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<Issue> GetOpenIssuesForProject(string sourceProjectKey)
        {
            return GetIssuesForProject(projectKey: sourceProjectKey, statuses: "OPEN,REOPENED");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<Issue> GetFalsePositiveIssuesForProject(string sourceProjectKey)
        {
            return GetIssuesForProject(projectKey: sourceProjectKey, resolutions: "FALSE-POSITIVE");
        }

        private int GetNumberOfIssuesForProject(string projectKey, string resolutions, string statuses)
        {
            Logger.LogDebug("Getting number of issues for project {0}", projectKey);

            var uri = GetIssuesSearchUri(1, 1, projectKey: projectKey, resolutions: resolutions, statuses: statuses);

            var responseContent = GetFromServer(uri);
            if (!string.IsNullOrEmpty(responseContent))
            {
                ApiIssuesSearchResult apiResult = JsonConvert.DeserializeObject<ApiIssuesSearchResult>(responseContent);
                return apiResult.Paging.Total;
            }

            return -1;
        }

        private IList<Issue> GetIssuesForComponent(Component component, string resolutions, string statuses)
        {
            Logger.LogDebug("Getting list of issues for component {0}", component.Key);

            var result = new List<Issue>();

            const int pageSize = 500;
            var pageIndex = 1;
            do
            {
                var uri = GetIssuesSearchUri(pageIndex, pageSize, componentKey: component.Key, resolutions: resolutions, statuses: statuses);

                var responseContent = GetFromServer(uri);
                if (!string.IsNullOrEmpty(responseContent))
                {
                    ApiIssuesSearchResult apiResult = JsonConvert.DeserializeObject<ApiIssuesSearchResult>(responseContent);
                    result.AddRange(apiResult.Issues);
                    if (apiResult.Paging.Total < (apiResult.Paging.PageIndex * apiResult.Paging.PageSize))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                pageIndex++;
            } while (true);

            Logger.LogDebug("\t{0} issues found", result.Count);

            return result;
        }

        public void UpdateIssueResolution(string issueKey, string newResolution, Comment[] comments)
        {
            if (newResolution == null) { throw new ArgumentNullException("newResolution"); }

            Logger.LogDebug("Updating resolution for issue {0} to {1}", issueKey, newResolution);

            string transition = string.Empty;

            switch (newResolution.ToUpperInvariant())
            {
                case "FALSE-POSITIVE":
                    transition = "falsepositive";
                    break;
                case "WONTFIX":
                    transition = "wontfix";
                    break;
                default:
                    throw new InvalidOperationException("Cannot update issue resolution to value " + newResolution);
            }

            var uri = new Uri(string.Format(CultureInfo.InvariantCulture,
                "{0}/api/issues/do_transition",
                SonarQubeUrl));
            PostToServer(uri, new[] {
                        new KeyValuePair<string, string>("issue", issueKey),
                        new KeyValuePair<string, string>("transition", transition)
                    });

            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    uri = new Uri(string.Format(CultureInfo.InvariantCulture,
                        "{0}/api/issues/add_comment",
                        SonarQubeUrl));
                    PostToServer(uri, new[] {
                        new KeyValuePair<string, string>("issue", issueKey),
                        new KeyValuePair<string, string>("text", comment.HtmlText)
                    });
                }
            }
        }

        private IList<Component> GetProjectComponents(string projectKey)
        {
            Logger.LogDebug("Getting list of components for project {0}", projectKey);

            var components = new List<Component>();

            const int pageSize = 500;
            var pageIndex = 1;
            do
            {
                var uri = new Uri(string.Format(CultureInfo.InvariantCulture,
                "{0}/api/components/tree?baseComponentKey={1}&qualifiers=DIR&p={2}&ps={3}",
                SonarQubeUrl,
                projectKey,
                pageIndex,
                pageSize));

                var responseContent = GetFromServer(uri);
                if (!string.IsNullOrEmpty(responseContent))
                {
                    ApiComponentsTreeResult apiResult = JsonConvert.DeserializeObject<ApiComponentsTreeResult>(responseContent);
                    components.AddRange(apiResult.Components);
                    if (apiResult.Paging.Total < (apiResult.Paging.PageIndex * apiResult.Paging.PageSize))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                pageIndex++;
            } while (true);

            Logger.LogDebug("\t{0} components found", components.Count);

            return components;
        }

        private Uri GetIssuesSearchUri(int pageIndex, int pageSize, string projectKey = null, string componentKey = null, string resolutions = null, string statuses = null)
        {
            var uriBuilder = new StringBuilder();
            uriBuilder.AppendFormat(CultureInfo.InvariantCulture,
                "{0}/api/issues/search?&p={1}&ps={2}",
                SonarQubeUrl,
                pageIndex,
                pageSize);

            if (!string.IsNullOrEmpty(projectKey))
            {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture,
                    "&projectKeys={0}",
                    projectKey);
            }

            if (!string.IsNullOrEmpty(componentKey))
            {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture,
                    "&componentKeys={0}",
                    componentKey);
            }

            if (!string.IsNullOrEmpty(resolutions))
            {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture,
                    "&resolutions={0}",
                    resolutions);
            }

            if (!string.IsNullOrEmpty(statuses))
            {
                uriBuilder.AppendFormat(CultureInfo.InvariantCulture,
                    "&statuses={0}",
                    statuses);
            }

            return new Uri(uriBuilder.ToString());
        }

        private string GetFromServer(Uri uri)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    AddHttpAuthorization(httpClient);
                    var response = httpClient.GetAsync(uri).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return responseContent;
                    }
                    else
                    {
                        Logger.LogError("Cannot get result from server! Uri = {0}, Status code = {1}, Response content: {2}",
                            uri,
                            response.StatusCode,
                            responseContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Cannot get result from server! Uri = {0}, Exception: {1}", uri, ex.ToString());
                throw;
            }

            return string.Empty;
        }

        private void PostToServer(Uri uri, KeyValuePair<string, string>[] parameters)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    AddHttpAuthorization(httpClient);

                    using (var content = new FormUrlEncodedContent(parameters))
                    {
                        var response = httpClient.PostAsync(uri, content).Result;
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Logger.LogError("Error when posting to the server! Uri = {0}, Status code = {1}, Response content: {2}",
                                uri,
                                response.StatusCode,
                                responseContent);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Cannot post to server! Uri: {0}, Exception: {0}", uri, ex.ToString());
                throw;
            }
        }

        private void AddHttpAuthorization(HttpClient httpClient)
        {
            var byteArray = Encoding.ASCII.GetBytes(UserName + ":" + Password);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
