using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqCopyResolution.Model.SonarQube
{
    public class ApiComponentsTreeResult
    {
        public Paging Paging { get; set; }
        public Component[] Components { get; set; }
    }
}
