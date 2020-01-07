using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public static class Function1
    {
        //http://localhost:7071/api/NewProject?project=ProjectAF1
        [FunctionName("NewProject")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var username = Environment.GetEnvironmentVariable("userProject", EnvironmentVariableTarget.Process);
            var password = Environment.GetEnvironmentVariable("passProject", EnvironmentVariableTarget.Process);

            log.LogInformation($"Username: {username}");
            log.LogInformation($"Password: {password}");

            // parse query parameter
            string project = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "project", true) == 0)
                .Value;
            

            if (project == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                project = data?.project;
            }

            else 
            {
                try
                {
                    Create_Project newProyect = new Create_Project();
                    newProyect.CreateProjectWithTaskAndAssignment(project);
                }
                catch 
                {
                    project = null;
                }
            }
            return project == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "The project " + project + " has been created");
        }

        //http://localhost:7071/api/CopyProject?project=ProjectAF1
        [FunctionName("CopyProject")]
        public static async Task<HttpResponseMessage> Run2([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string project = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "project", true) == 0)
                .Value;


            if (project == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                project = data?.project;
            }

            else
            {
                try
                {
                    Create_Project newProyect = new Create_Project();
                    newProyect.ReadAndCreateProject(project);
                }
                catch
                {
                    project = null;
                }
            }
            return project == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "The copy project from " + project + " has been created");

        }
    }
}

