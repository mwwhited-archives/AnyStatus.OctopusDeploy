using AnyStatus.API;
using Octopus.Client;
using Octopus.Client.Model;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AnyStatus.OctopusDeploy
{
    [Description("Octopus Deploy Monitor")]
    [DisplayName("Octopus Deployment")]
    [DisplayColumn("Deployment")]
    public class OctopusDeploymentWidget : Widget, IHealthCheck, ISchedulable, IWebPage
    {

        [Category("Deployment Definition")]
        [DisplayName("Project ID")]
        [XmlIgnore]
        public string ProjectId { get; internal set; }

        [Category("Deployment Definition")]
        [DisplayName("Project Name")]
        [Required]
        public string ProjectName { get; set; }

        [Category("Deployment Definition")]
        [DisplayName("Environment ID")]
        [XmlIgnore]
        public string EnvironmentId { get; internal set; }

        [Category("Deployment Definition")]
        [DisplayName("Environment Name")]
        [Required]
        public string EnvironmentName { get; set; }


        [Category("Deployment Definition")]
        [Description("Required. Octopus Deploy server (http://{server:port}) URL address.")]
        [Required]
        [Url]
        public string Url
        {
            get;
            set;
        }

        [Category("Deployment Definition")]
        [Description("Required. API Key")]
        [Required]
        public string ApiKey
        {
            get;
            set;
        }


        string _webUrl;
        string IWebPage.URL => this._webUrl ?? "";

        public override object Clone()
        {
            var widget = (OctopusDeploymentWidget)base.Clone();
            widget.ProjectId = "";
            widget.EnvironmentId = "";
            widget.State = State.Unknown;
            widget.StateText = "";
            return widget;
        }

        public Task Refresh()
        {
            return Task.Run(() =>
            {
                try
                {
                    var endpoint = new OctopusServerEndpoint(this.Url, this.ApiKey);
                    var repo = new OctopusRepository(endpoint);

                    if (string.IsNullOrWhiteSpace(this.ProjectId))
                    {
                        var project = repo.Projects.FindByName(this.ProjectName);
                        this.ProjectId = project.Id;
                    }
                    if (string.IsNullOrWhiteSpace(this.EnvironmentId))
                    {
                        var environment = repo.Environments.FindByName(this.EnvironmentName);
                        this.EnvironmentId = environment.Id;
                    }

                    var deployment = repo.Deployments.FindOne(d => d.ProjectId == this.ProjectId && d.EnvironmentId == this.EnvironmentId);

                    this._webUrl = new Uri(new Uri(this.Url), (deployment?.Link("Web")) ?? "/").AbsoluteUri;

                    var task = repo.Tasks.FindOne(t => t.Id == deployment?.TaskId);

                    if (task != null)
                    {
                        this.StateText = new
                        {
                            task.IsCompleted,
                            task.State,
                        }.ToString();

                        switch (task.State)
                        {
                            case TaskState.Queued:
                                this.State = State.Queued;
                                break;
                            case TaskState.Executing:
                                this.State = State.Running;
                                break;
                            case TaskState.Failed:
                                this.State = State.Failed;
                                break;
                            case TaskState.Canceled:
                                this.State = State.Canceled;
                                break;
                            case TaskState.TimedOut:
                                this.State = State.Rejected;
                                break;
                            case TaskState.Success:
                                this.State = State.Ok;
                                break;
                            case TaskState.Cancelling:
                                this.State = State.Canceled;
                                break;
                            default:
                                this.State = State.Unknown;
                                break;
                        }
                    }
                    else
                    {
                        this.StateText = $"Deployment Not Found: {this.Url} - {this.ProjectName}-{this.EnvironmentName}";
                        this.State = State.Invalid;
                    }
                }
                catch (Exception ex)
                {
                    this.StateText = ex.ToString();
                    this.State = State.Error;
                }
            });
        }
    }
}
