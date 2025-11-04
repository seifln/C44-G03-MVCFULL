using GymManagementDAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementDAL.Data.Configurations
{
    internal class MembershipConfigurations : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.Ignore(X => X.Id);
            builder.Property(X => X.CreatedAt)
                   .HasColumnName("StartDate")
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(X => X.Plan)
                   .WithMany(X => X.PlanMembers)
                   .HasForeignKey(X => X.PlanId);

            builder.HasOne(X => X.Member)
                   .WithMany(X => X.MemberPlans)
                   .HasForeignKey(X => X.MemberId);

            builder.HasKey(X => new { X.MemberId, X.PlanId });
        }
    }
}
