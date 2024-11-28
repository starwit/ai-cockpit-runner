using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using CliWrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cockpit_runner.docker;

internal class DockerFunctions()
{
    private string cockpitDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aicockpit");

    private MainWindow mainWindow;

    public DockerFunctions(MainWindow mainWindow): this()
    {
        this.mainWindow = mainWindow;
    }
    
    internal async void CheckIfDockerIsInstalled()
    {
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

