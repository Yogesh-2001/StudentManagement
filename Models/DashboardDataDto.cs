using System.Collections.Generic;

namespace StudentManagement.Models
{
    public class DashboardDataDto
    {
        public string AgentName { get; set; }
        public string AgentId { get; set; }
        public string DataCollectionLocation { get; set; }
        public List<TaskSummaryDto> TaskSummary { get; set; }
        public List<PoiDto> AssignedPOIs { get; set; }
    }
}
