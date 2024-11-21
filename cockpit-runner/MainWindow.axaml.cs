using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CliWrap;
using LibGit2Sharp;

namespace cockpit_runner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Height = 400;
        Width = 600;
        StartStopCockpit.IsEnabled = false;

        List<string> scenarios = new List<string>();
        scenarios.Add("Traffic");
        scenarios.Add("garbage mangement");
        scenarios.Add("Health Care");
        scenarios.Add("Event Management");
        SelectScenario.ItemsSource = scenarios;
    }

    public async void Button_Click(object sender, RoutedEventArgs args)
    {
        PreReqOutput.Text += "Checking pre requisites... \n";
        CheckIfDockerIsInstalled();
        CheckIfCodeiIsPresent();
    }

    public async void StartStopCockpit_Click(object sender, RoutedEventArgs args)
    {
        //TODO docker compose up
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(["compose", "up" , "-f", "demo-docker-compose.up"])
                .WithWorkingDirectory(".")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))            
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();

            PreReqOutput.Text += "Docker command is present \n";
            StartStopCockpit.IsEnabled = true;
            CheckIfCockpitIsRunning();
        } catch(System.ComponentModel.Win32Exception e)
        {
            PreReqOutput.Text += "Can't detect docker, please install Docker/Docker desktop ";
        }
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
            StartStopCockpit.IsEnabled = true;
            CheckIfCockpitIsRunning();
        } catch(System.ComponentModel.Win32Exception e)
        {
            PreReqOutput.Text += "Can't detect docker, please install Docker/Docker desktop ";
        }
    }

    private async void CheckIfCockpitIsRunning()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(["ps"])
                .WithWorkingDirectory(".")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();

            if (stdOut.Contains("cockpit"))
            {
                ActionOutput.Text += "Cockpit is running \n";
                StartStopCockpit.Content = "Stop Cockpit";
                return;
            }

            ActionOutput.Text += "Cockpit is not running \n";
        } catch(System.ComponentModel.Win32Exception e)
        {
            ActionOutput.Text += "Can't run Docker, please check if is installed ";
        }
    }

    private void CheckIfCodeiIsPresent()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var cockpitDir = Path.Combine(homeDir, ".aicockpit");
        Directory.CreateDirectory(cockpitDir);
        // check if dir is empty
        if(Directory.GetFiles(cockpitDir).Length == 0 && Directory.GetDirectories(cockpitDir).Length == 0)
        {
            Repository.Clone("https://github.com/starwit/ai-cockpit-deployment.git", cockpitDir);
            PreReqOutput.Text += "Code downloaded \n";

        } else
        {
            // TODO
        }
    }
}