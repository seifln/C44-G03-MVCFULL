using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementDAL.Entities
{
    [Table("MemberSession")]
    public class BookingEntity : BaseEntity
	{
		public bool IsAttended { get; set; }
		public int MemberId { get; set; }
		public Member Member { get; set; } = null!;
		public int SessionId { get; set; }
		public Session Session { get; set; } = null!;
	}
}
