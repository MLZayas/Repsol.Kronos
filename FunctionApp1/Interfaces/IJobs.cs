using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csom = Microsoft.ProjectServer.Client;

namespace FunctionApp1
{
    public interface IJobs : IDisposable
    {
        csom.ProjectContext GetContext(string pwa);
        void JobStateLog(csom.JobState jobState, string jobDescription);
        csom.PublishedProject GetProjectByName(string name, csom.ProjectContext context);
    }
}
