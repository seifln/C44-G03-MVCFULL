namespace GymManagementDAL.Entities
{
    public class Member : GymUser
    {
        public string Photo { get; set; } = null!;
        public HealthRecord HealthRecord { get; set; } = null!;
        public ICollection<BookingEntity> MemberSessions { get; set; } = null!;

        public ICollection<Membership> MemberPlans { get; set; } = null!;
    }
}
