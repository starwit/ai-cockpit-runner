using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CliWrap;
using cockpit_runner.docker;
using cockpit_runner.gitandcockpit;
using LibGit2Sharp;

namespace cockpit_runner;

public partial class MainWindow : Window
{
    public bool IsCockpitRunning { get; set; }

    private DockerFunctions df;
    private GitAndCockpit git = new GitAndCockpit();

    public MainWindow()
    {
        IsCockpitRunning = false;
        df = new DockerFunctions(this);
        InitializeComponent();
        Height = 400;
        Width = 600;

        df.CheckIfDockerIsInstalled();
        git.CheckIfCodeiIsPresent();
        List<string> scenarios = git.GetAvailableScenarios();
        SelectScenario.ItemsSource = scenarios;
        SelectScenario.SelectedIndex = 0;
    }

    private void StartStopCockpit_Click(object sender, RoutedEventArgs args)
    {
        df.CheckIfCockpitIsRunning();
        df.StartStopCockpit(IsCockpitRunning, SelectScenario.SelectedItem.ToString());
    }

    public void SetStartStopBtn(bool isRunning)
    {
        if(isRunning == true)
        {
            StartStopCockpitBtn.Content = "Stop Cockpit";
        } else
        {
            StartStopCockpitBtn.Content = "Start Cockpit";
        }
    }

    public void ToggleStartStopBtn()
    {
        if (StartStopCockpitBtn.Content == "Stop Cockpit")
        {
            StartStopCockpitBtn.Content = "Start Cockpit";
        } else 
        {
            StartStopCockpitBtn.Content = "Stop Cockpit";
        }
    }

    public void ToggleApplicationLinks()
    {
        CockpitLink.IsEnabled = !CockpitLink.IsEnabled;
        MinioLink.IsEnabled = !MinioLink.IsEnabled;
    }

    public void SetStatusPanel(string name, string version)
    {
        DockerName.Content = name;
        DockerVersion.Content = version;
    }
}