using GymManagementDAL.Data.Contexts;
using GymManagementDAL.Entities;
using GymManagementDAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagementDAL.Repositories.Classes
{
    public class MembershipRepository : GenericRepository<Membership>, IMembershipRepository
    {
        private readonly GymDbContext _dbContext;

        public MembershipRepository(GymDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Membership> GetAllMembershipsWithMemberAndPlan(Func<Membership, bool> predicate)
        {
            return FetchMembershipsWithRelatedEntities(predicate);
        }

        // Private helper method
        private IEnumerable<Membership> FetchMembershipsWithRelatedEntities(Func<Membership, bool> filterCondition)
        {
            return _dbContext.Memberships
                .Include(membership => membership.Plan)
                .Include(membership => membership.Member)
                .Where(filterCondition)
                .ToList();
        }
    }
}