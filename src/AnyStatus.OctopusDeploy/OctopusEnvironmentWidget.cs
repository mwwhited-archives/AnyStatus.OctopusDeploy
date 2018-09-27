//using AnyStatus.API;
//using Octopus.Client;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AnyStatus.OctopusDeploy
//{
//    [Description("Octopus Deploy Monitor")]
//    [DisplayName("Octopus Environment")]
//    [DisplayColumn("Deployment")]
//    public class OctopusEnvironmentWidget : Widget, IHealthCheck
//    {
//        public OctopusEnvironmentWidget() : base(true)
//        {
//        }

//        [Category("Deployment Definition")]
//        [Description("Check this box and force refresh to rebuild environment")]
//        public bool Rebuild { get; set; }

//        [Category("Deployment Definition")]
//        [DisplayName("Environment Name")]
//        [Required]
//        public string EnvironmentName { get; set; }


//        [Category("Deployment Definition")]
//        [Description("Required. Octopus Deploy server (http://{server:port}) URL address.")]
//        [Required]
//        [Url]
//        public string Url
//        {
//            get;
//            set;
//        }

//        [Category("Deployment Definition")]
//        [Description("Required. API Key")]
//        [Required]
//        public string ApiKey
//        {
//            get;
//            set;
//        }

//        public Task Refresh()
//        {
//            return Task.Run(() =>
//            {
//                if (this.Rebuild)
//                {
//                    this.IsExpanded = false;
//                    try
//                    {
//                        var endpoint = new OctopusServerEndpoint(this.Url, this.ApiKey);
//                        var repo = new OctopusRepository(endpoint);

//                        var environment = repo.Environments.FindByName(this.EnvironmentName);

//                        var machinesFolder = Items.FirstOrDefault(i => i.Name == "Machines");
//                        if (machinesFolder == null)
//                        {
//                            machinesFolder = new Folder { Name = "Machines" };
//                            Items.Add(machinesFolder);
//                        }
//                        machinesFolder.Items.Clear();

//                        var deploymentFolder = Items.FirstOrDefault(i => i.Name == "Deployments");
//                        if (deploymentFolder == null)
//                        {
//                            deploymentFolder = new Folder { Name = "Deployments" };
//                            Items.Add(deploymentFolder);
//                        }
//                        deploymentFolder.Items.Clear();

//                        var projects = repo.Projects.GetAll();
//                        var projectIds = projects.Select(p => p.Id).Distinct().ToArray();
//                        var deployments = from d in repo.Deployments.FindBy(projectIds, new[] { environment.Id }).Items
//                                          select new
//                                          {
//                                              EnvironmentName = environment.Name,
//                                              ProjectName = projects.FirstOrDefault(p => p.Id == d.ProjectId)?.Name,

//                                              EnvironmentId = environment.Id,
//                                              ProjectId = d.ProjectId,
//                                          };

//                        var machines = from m in repo.Machines.FindAll(path: environment.Link("Machines"))
//                                       select new
//                                       {
//                                           Name = $"{string.Join(",", m.Roles.Select(r => r))} ({m.Name})",
//                                           MachineName = m.Name,
//                                       };

//                        foreach (var machine in machines.Distinct())
//                        {
//                            machinesFolder.Items.Add(new OctopusMachineWidget
//                            {
//                                ApiKey = this.ApiKey,
//                                Url = this.Url,

//                                MachineName = machine.MachineName,
//                                Name = machine.Name,
//                            });
//                        }

//                        foreach (var deployment in deployments.Distinct())
//                        {
//                            deploymentFolder.Add(new OctopusDeploymentWidget
//                            {
//                                ApiKey = this.ApiKey,
//                                Url = this.Url,

//                                Name = deployment.ProjectName,

//                                EnvironmentName = deployment.EnvironmentName,
//                                EnvironmentId = deployment.EnvironmentId,

//                                ProjectName = deployment.ProjectName,
//                                ProjectId = deployment.ProjectId,
//                            });
//                        }

//                        this.Rebuild = false;
//                    }
//                    catch (Exception ex)
//                    {
//                        this.StateText = ex.ToString();
//                        this.State = State.Error;
//                    }
//                }
//            });
//        }
//    }
//}
