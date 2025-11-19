using System.Collections.Generic;

namespace StudentManagement.Models
{
    public class PoiDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
        public string TaskPriority { get; set; }
        public string TaskStatus { get; set; }
        public int Progress { get; set; }
        public bool RevisionRequired { get; set; }
        public string ContactNo { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string RevisionMessage { get; set; }
        public List<SubtaskDto> SubTasks { get; set; } = new List<SubtaskDto>();
    }
}
