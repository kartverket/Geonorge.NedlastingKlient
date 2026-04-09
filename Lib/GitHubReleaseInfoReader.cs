using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using RestSharp;

namespace Geonorge.MassivNedlasting
{
    public class GitHubReleaseInfoReader : IReleaseInfoReader
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RestClient _restClient;
        private GitHubReleaseInfo _latestReleaseInfo;

        public GitHubReleaseInfoReader()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _restClient = new RestClient("https://api.github.com/");
            _latestReleaseInfo = null;

            // Start fetching release info in background - don't block constructor
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    _latestReleaseInfo = await ReadGitHubLatestReleaseInfo();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to fetch release information in background");
                }
            });
        }

        private async System.Threading.Tasks.Task<GitHubReleaseInfo> ReadGitHubLatestReleaseInfo()
        {
            var request = new RestRequest("repos/kartverket/Geonorge.NedlastingKlient/releases/latest");
            //var request = new RestRequest("kartverket/Geonorge.NedlastingKlient/releases/latest");

            request.AddCookie("logged_in", "no", "/", "api.github.com");

            try
            {
                Log.Debug("Attempting to fetch GitHub release info from: {Url}", request.Resource);

                // Let's first try without deserialization to see the raw response
                RestResponse gitHubResponse = await _restClient.ExecuteAsync(request);

                Log.Debug("GitHub API Response - StatusCode: {StatusCode}, IsSuccessful: {IsSuccessful}", 
                    gitHubResponse.StatusCode, gitHubResponse.IsSuccessful);

                if (gitHubResponse.ErrorException != null)
                {
                    Log.Error(gitHubResponse.ErrorException, "RestSharp error occurred: {ErrorMessage}", gitHubResponse.ErrorMessage);
                    return null;
                }

                if (!gitHubResponse.IsSuccessful)
                {
                    Log.Error("GitHub API request failed - StatusCode: {StatusCode}, Content: {Content}", 
                        gitHubResponse.StatusCode, gitHubResponse.Content);
                    return null;
                }

                if (string.IsNullOrEmpty(gitHubResponse.Content))
                {
                    Log.Error("GitHub API returned empty content");
                    return null;
                }

                Log.Debug("Raw GitHub API Response: {Content}", gitHubResponse.Content);

                // Try to deserialize manually using Newtonsoft.Json
                try
                {
                    var releaseInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubReleaseInfo>(gitHubResponse.Content);

                    if (releaseInfo?.tag_name == null)
                    {
                        Log.Error("Could not parse tag_name from GitHub response");
                        return null;
                    }

                    Log.Debug("Successfully retrieved GitHub release info: {TagName}", releaseInfo.TagName);
                    return releaseInfo;
                }
                catch (Exception jsonEx)
                {
                    Log.Error(jsonEx, "Failed to deserialize GitHub response: {Content}", gitHubResponse.Content);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to retrieve necessary data from GitHub. Please check your internet connection.");
                return null;
            }
        }

        public Version GetLatestVersion()
        {
            if (_latestReleaseInfo?.TagName == null)
            {
                Log.Information("GitHub release information not yet available - returning current assembly version as fallback");
                // Return current assembly version as fallback
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version("1.0.0.0");
            }

            if (!Regex.IsMatch(_latestReleaseInfo.TagName, @"^v\d+.\d+.\d+$") && !Regex.IsMatch(_latestReleaseInfo.TagName, @"^v\d+.\d"))
            {
                Log.Error("Unexpected tag-name format: {TagName}", _latestReleaseInfo.TagName);
                throw new Exception($"Unexpected tag-name format: {_latestReleaseInfo.TagName}");
            }

            string versionNumber = _latestReleaseInfo.TagName.TrimStart('v');

            // Add .0 if it's missing to make it a valid version format
            if (!versionNumber.Contains("."))
                versionNumber += ".0.0";
            else if (versionNumber.Split('.').Length == 2)
                versionNumber += ".0";

            Log.Debug("Parsed version from GitHub tag '{TagName}': {Version}", _latestReleaseInfo.TagName, versionNumber);
            return new Version(versionNumber);
        }

        /// <summary>
        /// Returns true if GitHub release information has been successfully loaded
        /// </summary>
        public bool IsGitHubDataAvailable => _latestReleaseInfo?.TagName != null;

        private class GitHubReleaseInfo
        {
            public string tag_name { get; set; }  // GitHub API uses snake_case
            public string TagName => tag_name;    // Property for backward compatibility
        }
    }
}
