using CodInsightsCLI.Infra;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;

namespace CodInsightsCLI
{
    class Program
    {
        private static Project project;
        static void Main(InvocationContext invocationContext, string[] args = null)
        {
            project = new Project();

            ConsoleRenderer consoleRenderer = new ConsoleRenderer(
              invocationContext.Console,
              mode: invocationContext.BindingContext.OutputMode(),
              resetAfterRender: true
            );

            var repositories = project.GetProjectsForGitHub("RDPodcasting", "RDPodcasting", "https://api.github.com/users/RDPodcasting/repos").Result;

            var cmd = new RootCommand();
            cmd.AddCommand(project.NewProject(repositories,invocationContext,consoleRenderer));

            cmd.InvokeAsync(args).Wait();
        }
    }
}
