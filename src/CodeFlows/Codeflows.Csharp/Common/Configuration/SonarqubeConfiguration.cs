using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeflows.Csharp.Common.Configuration
{
    public class SonarqubeConfiguration
    {
        public required string Token { get; set; }
        public required string BaseUrl { get; set; }
    }
}
