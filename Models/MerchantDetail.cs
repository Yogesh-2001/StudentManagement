using System.Collections.Generic;

namespace StudentManagement.Models
{
    public class MerchantDetail
    {
      public string? merchantName { get; set; }
        public string? dataCollectionLocation { get; set; }  
        public TaskCount? taskCounts { get; set; }   

        public List<AssignedPOI>? assignedPOIs { get; set; } 
    }

    public class TaskCount
    {
       public int pending { get; set; }
        public int revision { get; set; }
        public int completedToday { get; set; }
        public int verified { get; set; }
    
    }
    public class AssignedPOI
    {
        public string? poiId { get; set; }   
        public string? poiName { get; set; } 

        public string? status { get; set; }  
        public string? priority { get; set; }    
    }
}
