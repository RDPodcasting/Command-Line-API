using CodInsightsCLI.Models;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Threading.Tasks;

namespace CodInsightsCLI.Domain
{
    public interface IProject
    {
        Task<List<Projects>> GetProjectsForGitHub(string name, string version,string url);
        Command NewProject(List<Projects> repositories,InvocationContext invocationContext,ConsoleRenderer consoleRenderer);
        void CreateProject(string project, string url);
    }
}
