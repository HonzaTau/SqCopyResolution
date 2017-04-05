using System;
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

        public Task<string> GetIssues(string projectKey, string resolutions)
        {
            Logger.LogInformation("Getting list of issues from SonarQube");
            Logger.LogDebug("\tprojectKey = {0}", projectKey);

            try
            {
                var uri = new Uri(string.Format(CultureInfo.InvariantCulture,
                    "{0}/api/issues/search?projectKeys={1}&resolutions={2}",
                    SonarQubeUrl,
                    projectKey,
                    resolutions));

                using (var httpClient = new HttpClient())
                {
                    AddHttpAuthorization(httpClient);
                    var response = httpClient.GetAsync(uri).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Logger.LogInformation(responseContent);
                    }
                    else
                    {
                        Logger.LogError("Cannot get list of issues! Status code = {0}, Response content: {1}",
                            response.StatusCode,
                            responseContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Cannot get list of issues! Exception: {0}", ex.ToString());
                throw;
            }

            return null;
        }

        private void AddHttpAuthorization(HttpClient httpClient)
        {
            var byteArray = Encoding.ASCII.GetBytes(UserName + ":" + Password);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
