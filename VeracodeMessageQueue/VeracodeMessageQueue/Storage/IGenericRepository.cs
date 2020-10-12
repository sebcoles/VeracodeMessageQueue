using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VeracodeMessageQueue.Storage
{
    public interface IGenericRepository<TEntity>
    {
        TEntity Create(TEntity entity);
        IQueryable<TEntity> GetAll();
        void Create(List<TEntity> entities);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
