using P3Core.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P3Core.Models.Repositories
{
    public interface IOrderRepository
    {
        void Save(Order order);
        Task<Order> GetOrder(int? id);
        Task<IList<Order>> GetOrders();
    }
}
