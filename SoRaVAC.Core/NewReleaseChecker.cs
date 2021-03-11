using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoRaVAC.Core
{
    public class NewReleaseChecker
    {
        private static NewReleaseChecker _instance = null;

        private bool alreadyChecked;
        public Release LastRelease { get; private set; }
        //public Release LastPreRelease { get; private set; }

        private NewReleaseChecker()
        {
            alreadyChecked = false;
            LastRelease = null;
            //LastPreRelease = null;
        }

        public static NewReleaseChecker GetInstance()
        {
            if (_instance == null)
            {
                _instance = new NewReleaseChecker();
            }
            return _instance;
        }

        public bool CheckForNewRelease(Assembly assembly)
        {
            // We force only one access to GitHub API per application launch
            if (!alreadyChecked)
            {
                Version localReleaseVersion = assembly.GetName().Version;

                var client = new GitHubClient(new ProductHeaderValue("SoRaVAC"));
                IReadOnlyList<Release> releases = Task.Run(() => client.Repository.Release.GetAll("AleronWen", "SoRaVAC")).Result;

                foreach (Release release in releases)
                {
                    Version releaseVerion = new Version(ExtractVersionFromTagName(release.TagName));

                    if (localReleaseVersion.CompareTo(releaseVerion) < 0)
                    {
                        // The release on GitHub is newer than than the local release
                        /*
                        if (LastPreRelease == null && release.Prerelease)
                        {
                            LastPreRelease = release;
                        }
                        else if (LastRelease == null && !release.Prerelease)
                        {
                            LastRelease = release;
                        }
                        */
                        LastRelease = release;
                        break;
                    }

                    // if we found the two latest releases we exit the loop
                    // if (LastRelease != null && LastPreRelease != null) break;
                }

                alreadyChecked = true;
            }

            return LastRelease != null;
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
    }
}
