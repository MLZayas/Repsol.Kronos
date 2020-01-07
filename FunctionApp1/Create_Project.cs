using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Microsoft.SharePoint.Client;
using csom = Microsoft.ProjectServer.Client;

namespace FunctionApp1
{
    public class Create_Project : ICreate_Project
    {
        public static string pwaInstanceUrl = "https://m365x960521.sharepoint.com/sites/pwa2";         // your pwa url
        private static csom.ProjectContext context;
        const int DEFAULTTIMEOUTSECONDS = 300;

        //private static string projectName = "MZD_Project_F1";
        private static string localResourceName = "MZD_Resource";
        private static string taskName = "MZD_Task";

        //private static string projectCFName = "Piloto Repsol - Pruebas";
        //private static string resourceCFName = "Recurso Piloto Repsol - Pruebas";
        //private static string taskCFName = "Task - Pruebas";


        /// <summary>
        /// Create a new project with one local resource, one enterprise resource, one task and one assignment
        /// </summary>
        public void CreateProjectWithTaskAndAssignment(string projectName)
        {
            //
            // Load csom context
            Jobs jobs = new Jobs();
            
            context = jobs.GetContext(pwaInstanceUrl);

            //
            // Create a project
            csom.PublishedProject project = context.Projects.Add(new csom.ProjectCreationInformation()
            {
                Name = projectName,
                Start = DateTime.Today,
                Description = "Created project from Azure Function"
            });
            csom.JobState jobState = context.WaitForQueue(context.Projects.Update(), DEFAULTTIMEOUTSECONDS);
            jobs.JobStateLog(jobState, "Creating project");

            //
            // Create a task in project
            context.Load(project, p => p,
                                    p => p.StartDate);    //load startdate of project 
            context.ExecuteQuery();

            csom.DraftProject draft = project.CheckOut();
            Guid taskId = Guid.NewGuid();
            csom.Task task = draft.Tasks.Add(new csom.TaskCreationInformation()
            {
                Id = taskId,
                Name = taskName,
                IsManual = false,
                Start = project.StartDate.AddDays(1),
                Duration = "3d"
            });

            draft.Update();

            //
            // Create a local resource and assign the task to him
            Guid resourceId = Guid.NewGuid();
            csom.ProjectResource resource = draft.ProjectResources.Add(new csom.ProjectResourceCreationInformation()
            {
                Id = resourceId,
                Name = localResourceName
            });

            draft.Update();

            csom.DraftAssignment assignment = draft.Assignments.Add(new csom.AssignmentCreationInformation()
            {
                ResourceId = resourceId,
                TaskId = taskId
            });

            draft.Update();
            jobState = context.WaitForQueue(draft.Publish(true), DEFAULTTIMEOUTSECONDS);    // draft.Publish(true) means publish and check in
            jobs.JobStateLog(jobState, "Creating task and assgin to a local resource");
        }
        public void ReadAndCreateProject(string projectName)
        {
            // Load csom context
            Jobs jobs = new Jobs();
            context = jobs.GetContext(pwaInstanceUrl);

            // Retrieve publish project named "New Project"
            // if you know the Guid of project, you can just call context.Projects.GetByGuid()
            csom.PublishedProject project = jobs.GetProjectByName(projectName, context);
            if (project == null)
            {
                Console.WriteLine("Failed to retrieve expected data, make sure you set up server data right. Press any key to continue....");
                return;
            }

            csom.DraftProject draft = project.CheckOut();

            // Retrieve project along with tasks & resources
            context.Load(draft, p => p.StartDate,
                                p => p.Description);
            context.Load(draft.Tasks, dt => dt.Where(t => t.Name == taskName));
            context.Load(draft.Assignments, da => da.Where(a => a.Task.Name == taskName &&
                                                                a.Resource.Name == localResourceName));
            context.Load(draft.ProjectResources, dp => dp.Where(r => r.Name == localResourceName));
            context.ExecuteQuery();

            // Make sure the data on server is right
            if (draft.Tasks.Count != 1 || draft.Assignments.Count != 1 || draft.ProjectResources.Count != 1)
            {
                Console.WriteLine("Failed to retrieve expected data, make sure you set up server data right. Press any key to continue....");
                Console.ReadLine();
                return;
            }

            // Since we already filetered and validated that the TaskCollection, ProjectResourceCollection and AssignmentCollection
            // contains just one filtered item each, we just get the first one.
            csom.DraftTask task = draft.Tasks.First();
            csom.DraftProjectResource resource = draft.ProjectResources.First();
            csom.DraftAssignment assignment = draft.Assignments.First();


            //
            // Create a project
            csom.PublishedProject copyProject = context.Projects.Add(new csom.ProjectCreationInformation()
            {
                Name = project.Name + "(copy)",
                Start = project.StartDate,
                Description = "Copy project from a Published one",
            });
            csom.JobState jobState = context.WaitForQueue(context.Projects.Update(), DEFAULTTIMEOUTSECONDS);
            jobs.JobStateLog(jobState, "Creating project");



            //
            // Copy and Create a task in project

            csom.DraftProject copyDraft = copyProject.CheckOut();
            Guid taskId = Guid.NewGuid();
            csom.Task copyTask = copyDraft.Tasks.Add(new csom.TaskCreationInformation()
            {
                Id = taskId,
                Name = task.Name,
                IsManual = task.IsManual,
                Start = task.Start,
                Duration = task.Duration
            });

            copyDraft.Update();


            //
            // Create a local resource and assign the task to him
            Guid resourceId = Guid.NewGuid();
            csom.ProjectResource copyResource = copyDraft.ProjectResources.Add(new csom.ProjectResourceCreationInformation()
            {
                Id = resourceId,
                Name = resource.Name
            });

            copyDraft.Update();

            csom.DraftAssignment copyAssignment = copyDraft.Assignments.Add(new csom.AssignmentCreationInformation()
            {
                ResourceId = resourceId,
                TaskId = taskId
            });

            copyDraft.Update();


            jobState = context.WaitForQueue(copyDraft.Publish(true), DEFAULTTIMEOUTSECONDS);
            jobs.JobStateLog(jobState, "Creating task and assgin to a local resource");
        }
        public void Dispose()
            {
                throw new NotImplementedException();
            }

    }
}

