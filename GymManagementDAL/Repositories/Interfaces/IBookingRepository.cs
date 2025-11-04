using GymManagementDAL.Entities;

namespace GymManagementDAL.Repositories.Interfaces
{
	public interface IBookingRepository : IGenericRepository<BookingEntity>
	{
		IEnumerable<BookingEntity> GetBySessionId(int sessionId);
	}
}
