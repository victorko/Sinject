using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
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
                Patcher.ProcessPatchesFile(this.PatchFilePath);
                this.Log.LogMessage("Patched from file '{0}' processed", this.PatchFilePath);
                return true;
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
