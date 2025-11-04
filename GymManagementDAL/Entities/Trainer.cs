using GymManagementDAL.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementDAL.Entities
{
    public class Trainer : GymUser
    {
        //Hire Date == CreatedAt of base entity 
        public Specialties Specialties { get; set; }
        public ICollection<Session> Sessions { get; set; } = null!;

    }
}
