using CodInsightsCLI.Domain;
using CodInsightsCLI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CodInsightsCLI.Infra
{
    public class Project : IProject
    {
        private List<Projects> nameRepositories;
        private HttpClient client;
        private ProductHeaderValue header;
        private ProductInfoHeaderValue userAgent;
        public Project()
        {
            nameRepositories = new List<Projects>();
            client = new HttpClient();
        }
        public async Task<List<Projects>> GetProjectsForGitHub(string name,string version,string url)
        {
            header = new ProductHeaderValue(name, version);
            userAgent = new ProductInfoHeaderValue(header);
            client.DefaultRequestHeaders.UserAgent.Add(userAgent);

            var response = await client
                                    .GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var projects = await response
                                        .Content
                                        .ReadAsStringAsync()
                                        .ConfigureAwait(false);

                dynamic repositories = JsonConvert.DeserializeObject(projects);

                foreach (var repository in repositories)
                {
                    nameRepositories.Add(new Projects()
                    {
                        Description = repository.name.ToString(),
                        Url = repository.git_url.ToString()
                    });
                }
            }

            return nameRepositories;
        }

        public Command NewProject(List<Projects> repositories,
        InvocationContext invocationContext,
        ConsoleRenderer consoleRenderer)
        {
            var cmd = new Command("new", "Comando para criar um novo projeto");

            foreach (var command in repositories)
            {
                var repositoryCommad = new Command(command.Description, $"Comando para criar um projeto à partir do template {command.Description}");
                repositoryCommad.AddOption(new Option(new[] { "--name", "-n" }, "Nome da solução")
                {
                    Argument = new Argument<string>
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });

                repositoryCommad.Handler = CommandHandler.Create<string>((name) => {
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Usage: new [template] [options]");
                        Console.WriteLine("\n");
                        Console.WriteLine("Options:");
                        Console.WriteLine("-n, --name <NomeDoSeuProjeto>");
                        return;
                    }

                    CreateProject(name, command.Url);
                });

                cmd.Add(repositoryCommad);
            }

            cmd.Handler = CommandHandler.Create(() =>
            {
                var table = new TableView<Projects>
                {
                    Items = repositories
                };

                Console.WriteLine("\n");
                Console.WriteLine("Usage: new [template]");
                Console.WriteLine("\n");

                table.AddColumn(template => template.Description, "Template");

                var screen = new ScreenView(consoleRenderer, invocationContext.Console) { Child = table };
                screen.Render();

                Console.WriteLine("-----");
                Console.WriteLine("\n");
                Console.WriteLine("Exemples:");
                Console.WriteLine($"  CodInsightsCLI new { repositories[0].Description } --name NomeDoSeuProjeto");
            });

            return cmd;
        }

        public void CreateProject(string project, string url)
        {
            using var powershell = PowerShell.Create();
            
                powershell.AddScript(@"cd C:\_test");
                powershell.AddScript($"mkdir {project}");
                powershell.AddScript($"cd {project}");
                powershell.AddScript($"git clone {url}");
                var results = powershell.Invoke();

            Console.WriteLine($"Projeto {project} criado com sucesso!");
        }
    }
}
