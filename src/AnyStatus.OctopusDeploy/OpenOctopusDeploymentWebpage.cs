using AnyStatus.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyStatus.OctopusDeploy
{
    public class OpenOctopusDeploymentWebpage : OpenWebPage<OctopusDeploymentWidget>
    {
        public OpenOctopusDeploymentWebpage(IProcessStarter ps) : base(ps)
        {
        }
    }
}
