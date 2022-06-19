using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Denrage.AdventureModule.Services
{
    public class WhiteboardService
    {
        public ConcurrentBag<Line> Lines = new ConcurrentBag<Line>();

        public void AddLines(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                this.Lines.Add(line);
            }
        }
    }
}
