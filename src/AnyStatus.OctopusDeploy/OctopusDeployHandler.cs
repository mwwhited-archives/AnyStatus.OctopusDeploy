using AnyStatus.API;
using System.Threading;
using System.Threading.Tasks;

namespace AnyStatus.OctopusDeploy
{
    public class OctopusDeployHandler : 
        ICheckHealth<OctopusMachineWidget>, 
        ICheckHealth<OctopusDeploymentWidget>
       //ICheckHealth<OctopusEnvironmentWidget>
    {
        public Task Handle(HealthCheckRequest<OctopusMachineWidget> request, CancellationToken cancellationToken)
        {
            return request.DataContext.Refresh();
        }

        public Task Handle(HealthCheckRequest<OctopusDeploymentWidget> request, CancellationToken cancellationToken)
        {
            return request.DataContext.Refresh();
        }

        //public Task Handle(HealthCheckRequest<OctopusEnvironmentWidget> request, CancellationToken cancellationToken)
        //{
        //    return request.DataContext.Refresh();
        //}
    }
}
