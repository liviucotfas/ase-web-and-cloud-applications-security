using System.Linq;

namespace MVCStore.Models
{
    public interface IProductRepository
    {
        IQueryable<Product> Products { get; }
    }
}
