using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlows.GenAi.OpenAi.DTOs
{
    public class CodeFlowIssue
    {
        public required string Message { get; set; }
        public required int StartLine { get; set; }
        public required int StartPos { get; set; }
        public required int EndLine { get; set; }
        public required int EndPos { get; set; }
    }
}
