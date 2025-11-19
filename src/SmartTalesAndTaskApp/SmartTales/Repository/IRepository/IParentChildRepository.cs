using SmartTales.Model;
using SmartTales.Data;

namespace SmartTales.Repository.IRepository
{
    public interface IParentChildRepository
    {
        Task<ParentChildModel> CreateAsync(ParentChildModel parentChild);
        Task<bool> DeleteAsync(int id);
        Task<ParentChildModel?> GetAsync(int id);
        Task<IEnumerable<ParentChildModel>> GetAllAsync();
        Task<ParentChildModel?> UpdateAsync(ParentChildModel parentChild);
        Task<IEnumerable<User>> GetChildrenByParentIdAsync(int parentId);
        Task<IEnumerable<User>> GetParentsByChildIdAsync(int childId);
        Task<bool> IsParentChildRelationshipExistsAsync(int parentId, int childId);
        Task<bool> RemoveParentChildRelationshipAsync(int parentId, int childId);
    }
}
