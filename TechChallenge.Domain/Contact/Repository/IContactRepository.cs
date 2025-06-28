using System.Linq.Expressions;
using TechChallange.Domain.Contact.Entity;

namespace TechChallange.Domain.Contact.Repository
{
    public interface IContactRepository
    {
        Task Create(ContactEntity contact);

        Task UpdateAsync(ContactEntity contact);

        Task<ContactEntity> GetByIdAsync(Guid id);

        Task<IEnumerable<ContactEntity>> GetByDddAsync(Guid regionId);

        Task<IEnumerable<ContactEntity>> GetAllPagedAsync(Expression<Func<ContactEntity, bool>> search, int pageSize, int page, Expression<Func<ContactEntity, dynamic>> orderDesc);

        Task<int> GetCountAsync(Expression<Func<ContactEntity, bool>> search);
    }
}
