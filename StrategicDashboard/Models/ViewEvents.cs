using System.Collections.Generic;

namespace OneJaxDashboard.Models
{
    public class ViewEvents
    {
        // If you actually want to keep this view model:
        public required List<Strategy> Goals { get; set; }        // or change this later to a list of goals
        public required List<Strategy> AllEvents { get; set; }    // events ARE Strategy instances
    }
}