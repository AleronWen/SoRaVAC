using Octokit;
using SoRaVAC.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoRaVAC.Core.Services
{
    public class NewReleaseChecker
    {
        private static NewReleaseChecker _instance = null;

        private bool alreadyChecked;
        public ReleaseInfo ReleaseInfo { get; private set; }

        private NewReleaseChecker()
        { 
            alreadyChecked = false;
            ReleaseInfo = null;
        }

        public static NewReleaseChecker GetInstance()
        {
            if (_instance == null)
            {
                _instance = new NewReleaseChecker();
            }
            return _instance;
        }

        public bool CheckForNewVerion(Assembly assembly)
        {
            // We force only one access to GitHub API per application launch
            if (!alreadyChecked)
            {
                Version localReleaseVersion = assembly.GetName().Version;

                var client = new GitHubClient(new ProductHeaderValue("SoRaVAC"));
                IReadOnlyList<Release> releases = Task.Run(() => client.Repository.Release.GetAll("AleronWen", "SoRaVAC")).Result;

                Release latestRelease = null;
                Release latestPreRelease = null;

                foreach (Release release in releases)
                {
                    Version releaseVerion = new Version(ExtractVersionFromTagName(release.TagName));

                    if (localReleaseVersion.CompareTo(releaseVerion) < 0)
                    {
                        // The release on GitHub is newer than than the local release
                        if (latestPreRelease == null && release.Prerelease)
                        {
                            latestPreRelease = release;
                        }
                        else if (latestRelease == null && !release.Prerelease)
                        {
                            latestRelease = release;
                        }
                    }

                    // if we found the two latest releases we exit the loop
                    if (latestRelease != null && latestPreRelease != null) break;
                }

                if (latestRelease == null && latestPreRelease == null)
                {
                    Debug.WriteLine("Something went bad, no release can be fetch.");
                }
                else if (latestRelease != null)
                {
                    ReleaseInfo = CreateReleaseInfoFromOctokitRelease(latestRelease);
                }
                else if (latestPreRelease != null)
                {
                    ReleaseInfo = CreateReleaseInfoFromOctokitRelease(latestPreRelease);
                }

                alreadyChecked = true;
            }

            return ReleaseInfo != null;
        }

        public static string ExtractVersionFromTagName(string tagName)
        {
            string version = null;
            string pattern = @"[0-9]+\.[0-9]+(\.[0-9]+){0,2}";

            Match match = Regex.Match(tagName, pattern);
            if (match.Success)
            {
                version = match.Value;
            }

            return version;
        }

        private ReleaseInfo CreateReleaseInfoFromOctokitRelease(Release release)
        {
            return new ReleaseInfo()
            {
                Name = release.Name,
                Version = release.TagName,
                Url = release.HtmlUrl,
                IsPreRelease = release.Prerelease
            };
        }
    }
}
