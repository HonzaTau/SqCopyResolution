using Newtonsoft.Json;
using SqCopyResolution.Model.SonarQube;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "I need to call Find method on the returned List, so I cannot use IList here.")]
        public List<Issue> GetIssuesForProject(string projectKey)
        {
            var result = new List<Issue>();

            var components = GetProjectComponents(projectKey);
            if (components != null)
            {
                foreach (var component in components)
                {
                    result.AddRange(GetIssuesForComponent(component));
                }
            }

            return result;
        }

        private IList<Issue> GetIssuesForComponent(Component component)
        {
            Logger.LogInformation("Getting list of issues for component {0}", component.Key);

            var result = new List<Issue>();

            const int pageSize = 500;
            var pageIndex = 1;
            do
            {
                var uri = new Uri(string.Format(CultureInfo.InvariantCulture,
                    "{0}/api/issues/search?componentKeys={1}&p={2}&ps={3}",
                    SonarQubeUrl,
                    component.Key,
                    pageIndex,
                    pageSize));

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

            Logger.LogInformation("\t{0} issues found", result.Count);

            return result;
        }

        private IList<Component> GetProjectComponents(string projectKey)
        {
            Logger.LogInformation("Getting list of components for project {0}", projectKey);

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
