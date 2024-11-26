using System;
using System.Text;
using CliWrap;

namespace cockpit_runner.docker;

internal class DockerFunctions()
{
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
            Console.WriteLine(stdOut);
            Console.WriteLine(stdErr);
        }
        catch (System.ComponentModel.Win32Exception e)
        {
            Console.WriteLine("Can't detect docker, please install Docker/Docker desktop " + e.Message);
        }
    }

}


internal class DockerPS
{
    public string Command { get; set; }
    public string CreatedAt { get; set; }
    public string ID { get; set; }
    public string Image { get; set; }
    public string Labels { get; set; }
    public string LocalVolumes { get; set; }
    public string Mounts { get; set; }
    public string Names { get; set; }
    public string Networks { get; set; }
    public string Ports { get; set; }
    public string RunningFor { get; set; }
    public string Size { get; set; }
    public string State { get; set; }
    public string Status { get; set; }
}


