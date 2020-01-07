using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public interface ICreate_Project : IDisposable
    {
        void CreateProjectWithTaskAndAssignment(string projectName);
        void ReadAndCreateProject(string projectName);
    }
}
