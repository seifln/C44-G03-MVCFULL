using GymManagementDAL.Entities;

namespace GymManagementDAL.Repositories.Interfaces
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        IEnumerable<Session> GetAllSessionsWithTrainerAndCategory(Func<Session, bool>? condition = null);
        Session? GetSessionWithTrainerAndCategory(int SessionId);

        int GetCountOfBookedSlots(int SessionId);
    }
}
