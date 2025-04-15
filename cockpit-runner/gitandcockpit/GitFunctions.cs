using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using LibGit2Sharp;
using Avalonia.Controls;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace cockpit_runner.gitandcockpit;

internal class GitAndCockpit
{
    private string cockpitDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aicockpit");
    string gitUrl = "https://github.com/starwit/ai-cockpit-deployment.git";

    public async Task GetTagList(ComboBox SelectTag)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "AICockpit-Runner");
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/starwit/ai-cockpit-deployment/tags");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    var tags = JsonSerializer.Deserialize(responseBody, GitHubTagContext.Default.ListGitHubTag);
                    var tagNames = new List<string>();
                    foreach (var tag in tags)
                    {
                        tagNames.Add(tag.Name);
                    }
                    SelectTag.ItemsSource = tagNames;
                    SelectTag.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message); 
            }   
        }
    }
    
    public void CheckIfCodeiIsPresent(string tagName)
    {
        if(Directory.Exists(cockpitDir) && Directory.GetFiles(cockpitDir).Length == 0)
        {
            Trace.WriteLine("Checkout directory exists but is empty");
            DeleteDirectory(cockpitDir);
        }
        Directory.CreateDirectory(cockpitDir);
        // check if dir is empty
        if (Directory.GetFiles(cockpitDir).Length == 0 && Directory.GetDirectories(cockpitDir).Length == 0)
        {
            try
            {
                var path = Repository.Clone(gitUrl, cockpitDir);

                // checkout selected tag
                var repositoryOptions = new RepositoryOptions { WorkingDirectoryPath = path };
                var checkoutOptions = new CheckoutOptions { };
                using (var repo = new Repository(path, repositoryOptions))
                {
                    Tag tag = repo.Tags[tagName];
                    Trace.WriteLine(tag);
                    Commit commit = tag.Target is Commit c ? c : ((TagAnnotation)tag.Target).Target as Commit;
                    Trace.WriteLine(commit);
                    repo.Reset(ResetMode.Hard, commit);
                    repo.CheckoutPaths(commit.Sha, new[] { "." }, new CheckoutOptions());
                }
                Trace.WriteLine("Checkout repository");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }

    public void CheckOutTag(string tag)
    {
        try
        {
            DeleteDirectory(cockpitDir);
            CheckIfCodeiIsPresent(tag);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    public void GetAvailableBinaryScenarios(ComboBox SelectScenario)
    {
        var scenarios = new List<string>();
        string[] binaryDataFolder = { cockpitDir, "docker-compose", "scenariodata", "binary_data" };
        var binaryDir = Path.Combine(binaryDataFolder);
        try
        {
            foreach (var dir in Directory.GetDirectories(binaryDir))
            {
                Trace.WriteLine($"{dir}");
                scenarios.Add(Path.GetFileName(dir));
            }
        } catch (Exception ex) 
        { 
            Trace.WriteLine(ex.Message);
        }

        SelectScenario.ItemsSource = scenarios;
        SelectScenario.SelectedIndex = 1;
    }

    public List<string> GetAvailableScenarioLanguages(string binaryScenario)
    {
        var scenarios = new List<string>();
        string[] folders = {cockpitDir, "docker-compose","scenariodata", "data_structures"};
        var scenarioDir = Path.Combine(folders);
        foreach (var dir in Directory.GetDirectories(scenarioDir, binaryScenario + "*"))
        {
            var folderName = Path.GetFileName(dir);
            var lang = folderName.Replace(binaryScenario + "-", "");
            scenarios.Add(Path.GetFileName(lang));
        }
        return scenarios;
    }

    private static void DeleteDirectory(string targetDir)
    {
        File.SetAttributes(targetDir, FileAttributes.Normal);

        string[] files = Directory.GetFiles(targetDir);
        string[] dirs = Directory.GetDirectories(targetDir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(targetDir, false);
    }
}