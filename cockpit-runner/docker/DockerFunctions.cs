using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using CliWrap;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cockpit_runner.docker;

internal class DockerFunctions()
{
    private string dockerDesktopPath = "";

    private string cockpitDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aicockpit");

    private MainWindow mainWindow;

    public DockerFunctions(MainWindow mainWindow): this()
    {
        this.mainWindow = mainWindow;
    }
    
    internal async void CheckIfDockerIsInstalled()
    {
        if(OperatingSystem.IsWindows())
        {
            var isInstalled = CheckDockerDesktop();
            if (isInstalled)
            {
                mainWindow.ActionOutput.Text += "Started Docker Desktop.";
            }
            else
            {
                mainWindow.ActionOutput.Text += "Could not start Docker Desktop, please install.";
            }
        }

        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(["info", "-f", "json"])
                .WithWorkingDirectory(".")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();

            // check two values as result here, name and server version
            var parsedTokens = JToken.Parse(stdOut);
            var n = parsedTokens.SelectToken("Name");
            var v = parsedTokens.SelectToken("ServerVersion");
            if (n == null || v == null)
            {
                mainWindow.SetStatusPanel("unkown", "unkown");
            } else
            {
                mainWindow.SetStatusPanel(n.ToString(), v.ToString());
            }
        }
        catch (System.ComponentModel.Win32Exception e)
        {
            Console.WriteLine("Can't detect docker, please install Docker/Docker desktop " + e.Message);
        }
    }

    private bool CheckDockerDesktop()
    {
        var keyname = @"HKEY_LOCAL_MACHINE\SOFTWARE\Docker Inc.\Docker\1.0";
        var valueName = "AppPath";
        if(Registry.GetValue(keyname, valueName, null) != null)
        {
            dockerDesktopPath = Registry.GetValue(keyname, valueName, null).ToString();
            if(File.Exists(dockerDesktopPath + "\\Docker Desktop.exe"))
            {
                var p = Process.Start(dockerDesktopPath + "\\Docker Desktop.exe");
                return true;
            }
        }

        return false;
    }

    internal async void CheckIfCockpitIsRunning()
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(["compose", "ls", "--format", "json"])
                .WithWorkingDirectory(".")
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();
            var runningComposeJobs = JsonConvert.DeserializeObject<List<DockerLS>>(stdOut);
            if (runningComposeJobs != null)
            {
                foreach (var job in runningComposeJobs)
                {
                    if (job.Name == "ai-cockpit")
                    {
                        mainWindow.IsCockpitRunning = true;
                        mainWindow.SetStartStopBtn(true);
                        Console.WriteLine("Cockpit is running");
                        return;
                    }
                }
                mainWindow.IsCockpitRunning = false;
                mainWindow.SetStartStopBtn(false);
            } else
            {
                mainWindow.ActionOutput.Text += "Couldn't talk to Docker. Make sure Docker Desktop is running.";
            }
        } catch(System.ComponentModel.Win32Exception e)
        {
            Console.WriteLine("Can't run Docker, please check if is installed " + e.Message);
        }
    }

    internal async void StartStopCockpit(bool isRunning, string scenario)
    {
        string[] arguments;
        if(isRunning)
        {
            arguments = new string[5]{"compose", "-f", "import-demo-docker-compose.yml", "down", "-v"};
        } else
        {
            arguments = new string[5]{"compose", "-f", "import-demo-docker-compose.yml", "up", "-d"};
        }

        Console.WriteLine("Commanding cockpit to " + arguments);
        var composeDirectory = Path.Combine(cockpitDir, "docker-compose");
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        try
        {
            var result = await Cli.Wrap("docker")
                .WithArguments(arguments)
                .WithWorkingDirectory(composeDirectory)
                .WithEnvironmentVariables(env => env
                    .Set("scenario", scenario))
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            var stdOut = stdOutBuffer.ToString();
            var stdErr = stdErrBuffer.ToString();
            PrintWithColor(stdErr, true);
            PrintWithColor(stdOut, false);
            mainWindow.ToggleStartStopBtn();
            mainWindow.IsCockpitRunning = !mainWindow.IsCockpitRunning;
            mainWindow.ToggleApplicationLinks();
        }
        catch (System.ComponentModel.Win32Exception e)
        {
            Console.WriteLine("Can't run Docker, please check if is installed " + e.Message);
        }
    }

    private void PrintWithColor(string text, bool isError)
    {
        if (isError)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(text);
        }
    }
}

internal class DockerLS
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string ConfigFiles { get; set; }
}

