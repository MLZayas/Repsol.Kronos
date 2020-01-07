using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using csom = Microsoft.ProjectServer.Client;

namespace FunctionApp1
{
    public class Jobs : IJobs
    {
        #region Utility functions
        /// <summary>
        /// Log to Console the job state for queued jobs
        /// </summary>
        /// <param name="jobState">csom jobstate</param>
        /// <param name="jobDescription">job description</param>
        public void JobStateLog(csom.JobState jobState, string jobDescription)
        {
            switch (jobState)
            {
                case csom.JobState.Success:
                    Console.WriteLine(jobDescription + " is successfully done.");
                    break;
                case csom.JobState.ReadyForProcessing:
                case csom.JobState.Processing:
                case csom.JobState.ProcessingDeferred:
                    Console.WriteLine(jobDescription + " is taking longer than usual.");
                    break;
                case csom.JobState.Failed:
                case csom.JobState.FailedNotBlocking:
                case csom.JobState.CorrelationBlocked:
                    Console.WriteLine(jobDescription + " failed. The job is in state: " + jobState);
                    break;
                default:
                    Console.WriteLine("Unkown error, job is in state " + jobState);
                    break;
            }
        }

        /// <summary>
        /// Get Publish project by name
        /// </summary>
        /// <param name="name">the name of the project</param>
        /// <param name="context">csom context</param>
        /// <returns></returns>
        public csom.PublishedProject GetProjectByName(string name, csom.ProjectContext context)
        {
            IEnumerable<csom.PublishedProject> projs = context.LoadQuery(context.Projects.Where(p => p.Name == name));
            context.ExecuteQuery();

            if (!projs.Any())       // no project found
            {
                return null;
            }
            return projs.FirstOrDefault();
        }

        /// <summary>
        /// Get csom ProjectContext by letting user type in username and password
        /// </summary>
        /// <param name="url">pwa website url string</param>
        /// <returns></returns>
        public csom.ProjectContext GetContext(string url)
        {
            csom.ProjectContext context = new csom.ProjectContext(url);

            var userName = Environment.GetEnvironmentVariable("userProject", EnvironmentVariableTarget.Process);
            var passWord = Environment.GetEnvironmentVariable("passProject", EnvironmentVariableTarget.Process);

            //userName = "admin@M365x960521.onmicrosoft.com";
            //passWord = "b5W2ChO81D";

            NetworkCredential netcred = new NetworkCredential(userName, passWord);
            SharePointOnlineCredentials orgIDCredential = new SharePointOnlineCredentials(netcred.UserName, netcred.SecurePassword);
            context.Credentials = orgIDCredential;

            return context;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}


