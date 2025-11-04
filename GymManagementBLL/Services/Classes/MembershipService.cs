using AutoMapper;
using GymManagementBLL.Services.Interfaces;
using GymManagementBLL.ViewModels.MembershipViewModels;
using GymManagementDAL.Entities;
using GymManagementDAL.Repositories.Interfaces;

namespace GymManagementBLL.Services.Classes
{
    public class MembershipService : IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MembershipService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public bool CreateMembership(CreateMemberShipViewModel CreatedMemberShip)
        {
            if (!ValidateMembershipCreation(CreatedMemberShip))
                return false;

            return ProcessMembershipCreation(CreatedMemberShip);
        }

        public bool DeleteMemberShip(int MemberId)
        {
            var activeMembership = FindActiveMembershipByMemberId(MemberId);

            if (activeMembership == null)
                return false;

            return RemoveMembershipFromRepository(activeMembership);
        }

        public IEnumerable<MemberShipViewModel> GetAllMemberShips()
        {
            var activeMemberships = RetrieveActiveMembershipsWithRelations();

            if (!activeMemberships.Any())
                return [];

            return TransformToMembershipViewModels(activeMemberships);
        }

        public IEnumerable<PlanSelectListViewModel> GetPlansForDropDown()
        {
            var activePlans = FetchActivePlans();
            return _mapper.Map<IEnumerable<PlanSelectListViewModel>>(activePlans);
        }

        public IEnumerable<MemberSelectListViewModel> GetMembersForDropDown()
        {
            var allMembers = FetchAllMembers();
            return _mapper.Map<IEnumerable<MemberSelectListViewModel>>(allMembers);
        }

        // Private helper methods
        private bool ValidateMembershipCreation(CreateMemberShipViewModel membershipModel)
        {
            if (!VerifyMemberExists(membershipModel.MemberId))
                return false;

            if (!VerifyPlanExists(membershipModel.PlanId))
                return false;

            if (CheckIfMemberHasActiveSubscription(membershipModel.MemberId))
                return false;

            return true;
        }

        private bool ProcessMembershipCreation(CreateMemberShipViewModel membershipModel)
        {
            try
            {
                var newMembership = BuildMembershipEntity(membershipModel);
                var subscriptionPlan = RetrievePlanById(membershipModel.PlanId);

                newMembership.EndDate = CalculateEndDate(subscriptionPlan!.DurationDays);

                _unitOfWork.MembershipRepository.Add(newMembership);
                return _unitOfWork.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        private Membership BuildMembershipEntity(CreateMemberShipViewModel model)
        {
            return _mapper.Map<Membership>(model);
        }

        private DateTime CalculateEndDate(int durationInDays)
        {
            return DateTime.Now.AddDays(durationInDays);
        }

        private Plan RetrievePlanById(int planId)
        {
            return _unitOfWork.GetRepository<Plan>().GetById(planId);
        }

        private Membership FindActiveMembershipByMemberId(int memberId)
        {
            var membershipRepo = _unitOfWork.MembershipRepository;
            return membershipRepo
                .GetAll(membership => membership.MemberId == memberId && membership.Status == "Active")
                .FirstOrDefault();
        }

        private bool RemoveMembershipFromRepository(Membership membership)
        {
            _unitOfWork.MembershipRepository.Delete(membership);
            return _unitOfWork.SaveChanges() > 0;
        }

        private IEnumerable<Membership> RetrieveActiveMembershipsWithRelations()
        {
            return _unitOfWork.MembershipRepository
                .GetAllMembershipsWithMemberAndPlan(membership => membership.Status == "Active");
        }

        private IEnumerable<MemberShipViewModel> TransformToMembershipViewModels(IEnumerable<Membership> memberships)
        {
            return _mapper.Map<IEnumerable<MemberShipViewModel>>(memberships);
        }

        private IEnumerable<Plan> FetchActivePlans()
        {
            return _unitOfWork.GetRepository<Plan>().GetAll(plan => plan.IsActive == true);
        }

        private IEnumerable<Member> FetchAllMembers()
        {
            return _unitOfWork.GetRepository<Member>().GetAll();
        }

        private bool VerifyMemberExists(int memberId)
        {
            return _unitOfWork.GetRepository<Member>().Exists(member => member.Id == memberId);
        }

        private bool VerifyPlanExists(int planId)
        {
            return _unitOfWork.GetRepository<Plan>().Exists(plan => plan.Id == planId);
        }

        private bool CheckIfMemberHasActiveSubscription(int memberId)
        {
            return _unitOfWork.MembershipRepository
                .Exists(membership => membership.MemberId == memberId && membership.Status == "Active");
        }
    }
}