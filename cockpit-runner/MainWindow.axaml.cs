using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CliWrap;
using cockpit_runner.docker;
using LibGit2Sharp;

namespace cockpit_runner;

public partial class MainWindow : Window
{
    private string cockpitDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aicockpit");
    private bool isCockpitRunning = false;

    private DockerFunctions df = new DockerFunctions();

    public MainWindow()
    {
        InitializeComponent();
        Height = 400;
        Width = 600;
        StartStopCockpitBtn.IsEnabled = false;

        CheckIfCodeiIsPresent();

        List<string> scenarios = new List<string>();
        scenarios.Add("Traffic");
        scenarios.Add("Garbage");
        scenarios.Add("Health");
        scenarios.Add("Event");
        SelectScenario.ItemsSource = scenarios;
    }

    public void PreRequisites_Click(object sender, RoutedEventArgs args)
    {
        PreReqOutput.Text += "Checking pre-requisites... \n";
        df.CheckIfDockerIsInstalled();
        //CheckIfDockerIsInstalled();
    }

    private async void CheckIfDockerIsInstalled()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(["info"])
                .WithWorkingDirectory(".")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();

            PreReqOutput.Text += "Docker command is present \n";
            StartStopCockpitBtn.IsEnabled = true;
            CheckIfCockpitIsRunning();
        }
        catch (System.ComponentModel.Win32Exception e)
        {
            PreReqOutput.Text += "Can't detect docker, please install Docker/Docker desktop " + e.Message;
        }
    }

    private void StartStopCockpit_Click(object sender, RoutedEventArgs args)
    {
        var command = "up";
        CheckIfCockpitIsRunning();
        if(isCockpitRunning == true)
        {
            command = "down";
        }

        var composeDirectory = Path.Combine(cockpitDir, "docker-compose");
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = Cli.Wrap("docker")
                .WithArguments(["compose", "-f", "import-demo-docker-compose.yml", command])
                .WithWorkingDirectory(composeDirectory)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();

            ActionOutput.Text += "Starting Docker compose \n";
            ActionOutput.Text += stdOut + "\n";
            ActionOutput.Text += stdErr + "\n";
            StartStopCockpitBtn.IsEnabled = true;
            if (command == "up")
            {
                StartStopCockpitBtn.Content = "Stop Cockpit";
            }
            else
            {
                StartStopCockpitBtn.Content = "Start Cockpit";
            }
        }
        catch (System.ComponentModel.Win32Exception e)
        {
            PreReqOutput.Text += "Can't detect docker, please install Docker/Docker desktop " + e.Message;
        }
    }

    private async void CheckIfCockpitIsRunning()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(["ps", "--format json"])
                .WithWorkingDirectory(".")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();

            ActionOutput.Text += stdOut + "\n";
            
            if (stdOut.Contains("ai-cockpit"))
            {
                ActionOutput.Text += "Cockpit is running \n";
                StartStopCockpitBtn.Content = "Stop Cockpit";
                isCockpitRunning = true;
                return;
            }
            ActionOutput.Text += "Cockpit is not running \n";
        } catch(System.ComponentModel.Win32Exception e)
        {
            ActionOutput.Text += "Can't run Docker, please check if is installed " + e.Message;
        }
        isCockpitRunning = false;
    }

    private void CheckIfCodeiIsPresent()
    {
        Directory.CreateDirectory(cockpitDir);
        // check if dir is empty
        if(Directory.GetFiles(cockpitDir).Length == 0 && Directory.GetDirectories(cockpitDir).Length == 0)
        {
            PreReqOutput.Text += "Checkout code \n";
            Repository.Clone("https://github.com/starwit/ai-cockpit-deployment.git", cockpitDir);

        } else
        {
            PreReqOutput.Text += "Code appears to be alread present, clean and checkout again";
            var signature = new Signature("Your Name", "your.email@example.com", DateTimeOffset.Now);
            var repo = new Repository(cockpitDir);
            Commands.Pull(repo, signature, new PullOptions());
        }
    }

    private void setAttributesNormal(DirectoryInfo dir)
    {
        foreach (var subDir in dir.GetDirectories())
            setAttributesNormal(subDir);
        foreach (var file in dir.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
        }
    }
}