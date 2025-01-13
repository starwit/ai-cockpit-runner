using System;
using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;

namespace cockpit_runner.gitandcockpit;

internal class GitAndCockpit
{
    private string cockpitDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aicockpit");
    string gitUrl = "https://github.com/starwit/ai-cockpit-deployment.git";
    
    public void CheckIfCodeiIsPresent()
    {
        Directory.CreateDirectory(cockpitDir);
        // check if dir is empty
        if(Directory.GetFiles(cockpitDir).Length == 0 && Directory.GetDirectories(cockpitDir).Length == 0)
        {
            // if not clone repo
            Repository.Clone(gitUrl, cockpitDir);
        } else
        {
            // pull latest code
            var signature = new Signature("Your Name", "your.email@example.com", DateTimeOffset.Now);
            var repo = new Repository(cockpitDir);
            try
            {
                Commands.Pull(repo, signature, new PullOptions());
            } catch (LibGit2SharpException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }

    public List<string> GetAvailableBinaryScenarios()
    {
        var scenarios = new List<string>();
        string[] binaryDataFolder = { cockpitDir, "docker-compose", "scenariodata", "binary_data" };
        var binaryDir = Path.Combine(binaryDataFolder);
        foreach (var dir in Directory.GetDirectories(binaryDir))
        {
            scenarios.Add(Path.GetFileName(dir));
        };

        return scenarios;
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
}