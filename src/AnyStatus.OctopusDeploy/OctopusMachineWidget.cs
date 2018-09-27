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
    [DisplayName("Octopus Server")]
    [DisplayColumn("Deployment")]
    public class OctopusMachineWidget : Widget, IHealthCheck, ISchedulable
    {
        [Category("Deployment Definition")]
        [DisplayName("Machine Name")]
        [Required]
        public string MachineName { get; set; }

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

        public override object Clone()
        {
            var widget = (OctopusMachineWidget)base.Clone();
            //widget.MachineName = "";
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
                    var machine = repo.Machines.FindByName(this.MachineName);

                    if (machine != null)
                    {
                        this.StateText = new
                        {
                            machine.IsInProcess,
                            machine.HealthStatus,
                            machine.Status,
                        }.ToString();

                        if (machine.HealthStatus != MachineModelHealthStatus.Healthy || machine.Status != MachineModelStatus.Online)
                        {
                            this.State = State.Failed;
                        }
                        else
                        {
                            this.State = machine.IsInProcess ? State.Running : State.Ok;
                        }
                    }
                    else
                    {
                        this.StateText = $"Machine Not Found: {this.Url} - {this.MachineName}";
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
