using SmartTales.Data;
using SmartTales.Model;
using SmartTales.Repository.IRepository;
using SQLite;

namespace SmartTales.Repository
{
    public class ParentChildRepository : IParentChildRepository
    {
        private readonly LocalDbService _localDbService;

        public ParentChildRepository(LocalDbService localDbService)
        {
            _localDbService = localDbService ?? throw new ArgumentNullException(nameof(localDbService));
        }

        public async Task<ParentChildModel> CreateAsync(ParentChildModel parentChild)
        {
            if (parentChild == null) throw new ArgumentNullException(nameof(parentChild));
            await _localDbService.GetConnection().InsertAsync(parentChild);
            return parentChild;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var obj = await _localDbService.GetConnection().FindAsync<ParentChildModel>(id);
            if (obj == null) return false;

            await _localDbService.GetConnection().DeleteAsync(obj);
            return true;
        }

        public async Task<ParentChildModel?> GetAsync(int id)
        {
            return await _localDbService.GetConnection().FindAsync<ParentChildModel>(id);
        }

        public async Task<IEnumerable<ParentChildModel>> GetAllAsync()
        {
            return await _localDbService.GetConnection().Table<ParentChildModel>().ToListAsync();
        }

        public async Task<ParentChildModel?> UpdateAsync(ParentChildModel parentChild)
        {
            if (parentChild == null) throw new ArgumentNullException(nameof(parentChild));

            var objFromDb = await _localDbService.GetConnection().FindAsync<ParentChildModel>(parentChild.Id);
            if (objFromDb == null) return null;

            objFromDb.ParentId = parentChild.ParentId;
            objFromDb.ChildId = parentChild.ChildId;
            objFromDb.IsActive = parentChild.IsActive;

            await _localDbService.GetConnection().UpdateAsync(objFromDb);
            return objFromDb;
        }

        public async Task<IEnumerable<User>> GetChildrenByParentIdAsync(int parentId)
        {
            var query = @"
                SELECT u.* FROM User u
                INNER JOIN ParentChild pc ON u.ID = pc.ChildId
                WHERE pc.ParentId = ? AND pc.IsActive = 1 AND u.Role = 'Kid'";

            return await _localDbService.GetConnection().QueryAsync<User>(query, parentId);
        }

        public async Task<IEnumerable<User>> GetParentsByChildIdAsync(int childId)
        {
            var query = @"
                SELECT u.* FROM User u
                INNER JOIN ParentChild pc ON u.ID = pc.ParentId
                WHERE pc.ChildId = ? AND pc.IsActive = 1 AND u.Role = 'Parent'";

            return await _localDbService.GetConnection().QueryAsync<User>(query, childId);
        }

        public async Task<bool> IsParentChildRelationshipExistsAsync(int parentId, int childId)
        {
            var relationship = await _localDbService.GetConnection()
                .Table<ParentChildModel>()
                .Where(pc => pc.ParentId == parentId && pc.ChildId == childId && pc.IsActive)
                .FirstOrDefaultAsync();

            return relationship != null;
        }

        public async Task<bool> RemoveParentChildRelationshipAsync(int parentId, int childId)
        {
            var relationship = await _localDbService.GetConnection()
                .Table<ParentChildModel>()
                .Where(pc => pc.ParentId == parentId && pc.ChildId == childId && pc.IsActive)
                .FirstOrDefaultAsync();

            if (relationship == null) return false;

            relationship.IsActive = false;
            await _localDbService.GetConnection().UpdateAsync(relationship);
            return true;
        }
    }
}
