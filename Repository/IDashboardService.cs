using StudentManagement.Models;
using System.Threading.Tasks;

namespace StudentManagement.Repository
{
    public interface IDashboardService
    {

        Task<ServiceResponse<MerchantDetail>> GetDashboardDetails(string mobileNo);
        Task<DashboardDataDto> GetDashboardDataAsync(string agentId);


        Task<bool> CreatePoiAssignmentAsync(PoiAssignmentInputDto assignment);
    }
}
