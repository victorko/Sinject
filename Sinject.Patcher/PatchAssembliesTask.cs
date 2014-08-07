using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sinject.Patcher
{
    public class PatchAssembliesTask : Task
    {
        [Required]
        public string PatchFilePath { get; set; }

        public override bool Execute()
        {
            try
            {
                var assemblies = PatchesFile.Load(this.PatchFilePath);
                foreach (var assembly in assemblies)
                {
                    var assemblyPath = GetDependentAssemblyPath(assembly.Name);
                    var patcher = new Patcher(assemblyPath);
                    foreach (var type in assembly.Types)
                    {
                        foreach (var method in type.Methods)
                        {
                            patcher.Patch(type.Name, method);
                        }
                    }
                    patcher.Save(assemblyPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }
        }

        private string GetDependentAssemblyPath(string assemblyName)
        {
            var project = new Project(this.BuildEngine.ProjectFileOfTaskNode);
            var items = project.GetItems("ProjectReference");
            foreach (var item in items)
            {
                var refProject = new Project(item.EvaluatedInclude);
                if (refProject.GetPropertyValue("AssemblyName") == assemblyName)
                {
                    return Path.Combine(
                        refProject.GetPropertyValue("TargetDir"),
                        refProject.GetPropertyValue("TargetName") + refProject.GetPropertyValue("TargetExt"));
                }
            }
            throw new Exception("Invalid assembly name");
        }
    }
}
