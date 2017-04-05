using Newtonsoft.Json;
using SqCopyResolution.Model.SonarQube;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SqCopyResolution.Services
{
    public class SonarQubeProxy
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

        public Dictionary<Component, List<Issue>> GetIssues(string projectKey, string resolutions)
        {
            var result = new Dictionary<Component, List<Issue>>();

            var components = GetProjectComponents(projectKey, resolutions);
            if (components != null)
            {
                const int pageSize = 500;
                foreach (var component in components)
                {
                    result.Add(component, new List<Issue>());

                    Logger.LogInformation("Getting list of issues for component {0}", component.Path);

                    var pageIndex = 1;
                    do
                    {
                        var uri = new Uri(string.Format(CultureInfo.InvariantCulture,
                            "{0}/api/issues/search?projectKeys={1}&resolutions={2}&componentKeys={3}&p={4}&ps={5}",
                            SonarQubeUrl,
                            projectKey,
                            resolutions,
                            component.Key,
                            pageIndex,
                            pageSize));

                        var responseContent = GetFromServer(uri);
                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            ApiIssuesSearchResult apiResult = JsonConvert.DeserializeObject<ApiIssuesSearchResult>(responseContent);
                            result[component].AddRange(apiResult.Issues);
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

                    Logger.LogInformation("\t{0} issues found", result[component].Count);
                }
            }

            return result;
        }

        private IList<Component> GetProjectComponents(string projectKey, string resolutions)
        {
            Logger.LogInformation("Getting list of components with issues from SonarQube...");

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

            Logger.LogInformation("\t{0} components found", components.Count);

            return components;
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
                Logger.LogError("Cannot get result from server! Exception: {0}", ex.ToString());
                throw;
            }

            return string.Empty;
        }

        private void AddHttpAuthorization(HttpClient httpClient)
        {
            var byteArray = Encoding.ASCII.GetBytes(UserName + ":" + Password);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
