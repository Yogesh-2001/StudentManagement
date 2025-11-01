using System.Collections.Generic;

namespace StudentManagement.Models
{
    public class SubmitPOIRequest : BasePOIRequest
    {
       // public POIFieldData fieldData {  get; set; }
        public Dictionary<string, object>? FieldData { get; set; }
    }
}
