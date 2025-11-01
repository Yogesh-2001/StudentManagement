using StudentManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentManagement.Repository
{
    public interface IPOIService
    {
        Task<ServiceResponse<List<POIField>>> GetPoiFieldsAsync(string poiId);

        Task<ServiceResponse<object>> SubmitPoiAsync(SubmitPOIRequest request);
    }
}
