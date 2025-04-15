using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using cockpit_runner.docker;
using cockpit_runner.gitandcockpit;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace cockpit_runner;

public partial class MainWindow : Window
{
    public bool IsCockpitRunning { get; set; }

    private DockerFunctions df;
    private GitAndCockpit git = new GitAndCockpit();
    string standardTag = "0.2.10-4-kic";

    public MainWindow()
    {
        IsCockpitRunning = false;
        df = new DockerFunctions(this);
        InitializeComponent();
        Height = 400;
        Width = 650;

        df.CheckIfDockerIsInstalled();
        ActionOutput.Text += "Check and prepare AI Cockpit code... \n";
        git.GetTagList(SelectTag).GetAwaiter();
        git.CheckIfCodeiIsPresent(standardTag);
        ActionOutput.Text += "AI Cockpit code ready\n";

        git.GetAvailableBinaryScenarios(SelectScenario);
        df.CheckIfCockpitIsRunning();
    }

    private void BinaryScenarioSelected(object sender, SelectionChangedEventArgs e)
    {
        Trace.WriteLine(string.Join(" ", SelectScenario.Items));
        if(SelectScenario.SelectedItem != null)
        {
            var scenario = SelectScenario.SelectedItem.ToString();

            List<string> scenarioLanguages = git.GetAvailableScenarioLanguages(scenario);
            SelectLanguage.ItemsSource = scenarioLanguages;
            SelectLanguage.SelectedIndex = 0;
            SelectLanguage.IsVisible = true;
        }
    }

    private void StartStopCockpit_Click(object sender, RoutedEventArgs args)
    {
        df.CheckIfCockpitIsRunning();
        df.StartStopCockpit(IsCockpitRunning, SelectScenario.SelectedItem.ToString(), SelectLanguage.SelectedItem.ToString());
    }

    private async void SwitchVersion_Click(object sender, RoutedEventArgs args)
    {
        var alertBox = MessageBoxManager
            .GetMessageBoxStandard("Version Switch", 
                "This will checkout a different AI Cockpit version. All local modifications will be lost. Continue?", 
                MsBox.Avalonia.Enums.ButtonEnum.OkCancel);
        var result = await alertBox.ShowWindowDialogAsync(this);
        if(result.Equals(ButtonResult.Ok))
        {
            ActionOutput.Text += "Delete existing code and checkout tag... \n";
            git.CheckOutTag(SelectTag.SelectedItem.ToString());
            git.GetAvailableBinaryScenarios(SelectScenario);
        }
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

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        df.StartStopCockpit(true, SelectScenario.SelectedItem.ToString(), SelectLanguage.SelectedItem.ToString());
    }
}