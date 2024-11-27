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
            Commands.Pull(repo, signature, new PullOptions());
        }
    }

    public List<string> GetAvailableScenarios()
    {
        var scenarios = new List<string>();
        string[] folders = {cockpitDir, "docker-compose","scenariodata", "data_structures"};
        var scenarioDir = Path.Combine(folders);
        foreach (var dir in Directory.GetDirectories(scenarioDir))
        {
            scenarios.Add(Path.GetFileName(dir));
        }
        return scenarios;
    }
}