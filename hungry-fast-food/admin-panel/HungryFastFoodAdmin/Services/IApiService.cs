// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\IApiService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using HungryFastFoodAdmin.Models;

namespace HungryFastFoodAdmin.Services
{
    public interface IApiService
    {
        Task<ApiService.ApiResult<bool>> SyncCategory(Category category);
        Task<ApiService.ApiResult<bool>> UpdateCategory(string id, Category category);
        Task<ApiService.ApiResult<bool>> DeleteCategory(string id);
        Task<ApiService.ApiResult<bool>> SyncProduct(Product product);
        Task<ApiService.ApiResult<bool>> UpdateProduct(string id, Product product);
        Task<ApiService.ApiResult<bool>> DeleteProduct(string id);
        Task<ApiService.ApiResult<bool>> UpdateProductAvailability(string id, bool isActive);
        Task<List<Order>> PullNewOrders(string since);
        Task<ApiService.ApiResult<bool>> SyncOrder(Order order);
        Task<ApiService.ApiResult<bool>> PushSyncItems(List<DatabaseService.SyncQueueItem> items);
        Task<ApiService.ApiResult<bool>> PushFullSync(object payload);
        Task<string> GetSyncStatus();
    }
}
