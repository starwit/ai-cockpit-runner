# Runner for AI Cockpit
This application aims at providing a tool to run a demo of AI Cockpit / KI-Cockpit on your computer. It is a desktop application that remote controls Docker for non technical users. If you are a technical user, please look at [deployment repo](https://github.com/starwit/ai-cockpit-deployment) where you can find various ways to run AI Cockpit.

Software in this repository is part of a public funded research project named KI-Cockpit (AI cockpit) and more information can be found here https://www.kicockpit.eu/.

## Repo structure
Application is written in C# and makes use of the .Net eco-system. So you should have some experience with .Net, if you want to compile code. If you want to learn C#, you may have a look [here](https://github.com/starwit-trainings/csharp-basics).

## How to build
You need to install .Net runtime for your operating system. A good start to look at is [here](https://dotnet.microsoft.com/en-us/download). Once you installed .Net the __dotnet__ command becomes available.

To build software open your favorite terminal and run from the base folder of this repo, the following commands:

```bash
    cd cockpit-runner
    dotnet build
```

You can also start software like so.
```bash
    dotnet run
```

## How to run

Github start page contains a link to releases. Select latest release, download exe file and run. Application has no installation routine yet.

## Contact & Contribution
This project was funded by the government of the federal republic of Germany. It is part of a research project aiming to keep _humans in command_ and is organized by the Federal Ministry of Labour and Social Affairs.

![BMAS](doc/BMAS_Logo.svg)

# License

Software in this repository is licensed under the AGPL-3.0 license. See [license agreement](LICENSE) for more details.