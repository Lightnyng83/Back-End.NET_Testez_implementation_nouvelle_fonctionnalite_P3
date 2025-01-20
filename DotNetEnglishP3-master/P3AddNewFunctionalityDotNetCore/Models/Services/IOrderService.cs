using P3Core.Models.Entities;
using P3Core.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P3Core.Models.Services
{
    public interface IOrderService
    {
        void SaveOrder(OrderViewModel order);
        Task<Order> GetOrder(int id);
        Task<IList<Order>> GetOrders();
    }
}
