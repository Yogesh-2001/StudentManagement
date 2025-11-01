using StudentManagement.Models;
using System.Threading.Tasks;

namespace StudentManagement.Repository
{
    public interface IDashboardService
    {

        Task<ServiceResponse<MerchantDetail>> GetDashboardDetails(string mobileNo);
    }
}
