using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class POIField
    {
        [Required]
        public string fieldName { get; set; }
        [Required]
        public string fieldType { get; set; }
        public int? minLength { get; set; }
        public int? maxLength { get; set; }
        [Required]
        public bool isMandatory { get; set; }   

        public List<string?> values { get; set; }   
        
        public int? uploadLimit { get; set; }    

        public List<string?> acceptedFormats { get; set; }

        public string? description { get; set; } 
    }
     
    public class POIFieldData
    {
        public string siteName {  get; set; }   
        public string type { get; set; }
        public List<string> availableFacilities {  get; set; }  
        public PhotoRequirement photoRequirement { get; set; }

    }

    public class PhotoRequirement
    {
        public List<string> selectedCategories { get; set; }    
        public UploadedPhotoDetails uploadedPhotos {  get; set; }   
    }

    public class UploadedPhotoDetails
    {
        public List<string> exterior_shot { get; set; }
        public List<string> interior_shot { get; set; }
        public List<string> architectural_details { get; set; }

    }
}
