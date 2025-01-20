using P3Core.Models.Entities;
using System.Collections.Generic;

namespace P3Core.Models
{
    public interface ICart
    {
        void AddItem(Product product, int quantity);

        void RemoveLine(Product product);

        void Clear();

        double GetTotalValue();

        double GetAverageValue();

        IEnumerable<CartLine> Lines { get; }
    }
}