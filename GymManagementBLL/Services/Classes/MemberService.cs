using AutoMapper;
using GymManagementBLL.Services.AttachmentService;
using GymManagementBLL.Services.Interfaces;
using GymManagementBLL.ViewModels.MemberViewModel;
using GymManagementDAL.Entities;
using GymManagementDAL.Repositories.Interfaces;

namespace GymManagementBLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;

        public MemberService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }
        public bool CreateMember(CreateMemberViewModel CreatedMember)
        {
            try
            {
                var Repo = _unitOfWork.GetRepository<Member>();

                if (IsEmailExists(CreatedMember.Email))
                    return false;
                if (IsPhoneExists(CreatedMember.Phone))
                    return false;


                var photoPath = _attachmentService.Upload(CreatedMember.PhotoFile, "members");

                if (string.IsNullOrEmpty(photoPath))
                    return false;

                var MemberEntity = _mapper.Map<Member>(CreatedMember);
                MemberEntity.Photo = photoPath;
                Repo.Add(MemberEntity);
                return _unitOfWork.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }


        }
        public IEnumerable<MemberViewModel> GetAllMembers()
        {
            var Members = _unitOfWork.GetRepository<Member>().GetAll();
            if (!Members.Any()) return [];
            return _mapper.Map<IEnumerable<MemberViewModel>>(Members);
        }
        public MemberViewModel? GetMemberDetails(int MemberId)
        {
            var member = _unitOfWork.GetRepository<Member>().GetById(MemberId);

            if (member is null) return null;

            var viewModel = _mapper.Map<MemberViewModel>(member);

            var activeMemberShip = _unitOfWork.GetRepository<Membership>()
                .GetAll(MP => MP.MemberId == MemberId && MP.Status == "Active").FirstOrDefault();

            if (activeMemberShip is not null)
            {
                var activePlan = _unitOfWork.GetRepository<Plan>().GetById(activeMemberShip.PlanId);

                viewModel.PlanName = activePlan?.Name;
                viewModel.MembershipStartDate = activeMemberShip.CreatedAt.ToShortDateString();
                viewModel.MembershipEndDate = activeMemberShip.EndDate.ToShortDateString();
            }

            return viewModel;
        }
        public HealthRecordViewModel? GetMemberHealthRecord(int MemberId)
        {
            var MemberHealthRecord = _unitOfWork.GetRepository<HealthRecord>().GetById(MemberId);
            if (MemberHealthRecord is null) return null;

            return _mapper.Map<HealthRecordViewModel>(MemberHealthRecord);
        }
        public MemberToUpdateViewModel? GetMemberToUpdate(int MemberId)
        {
            var member = _unitOfWork.GetRepository<Member>().GetById(MemberId);
            if (member is null) return null;
            return _mapper.Map<MemberToUpdateViewModel>(member);
        }
        public bool RemoveMember(int MemberId)
        {
            var Repo = _unitOfWork.GetRepository<Member>();
            var Member = Repo.GetById(MemberId);
            if (Member is null) return false;
            var sessionIds = _unitOfWork.GetRepository<BookingEntity>().GetAll(
               b => b.MemberId == MemberId).Select(S => S.SessionId); // 1 5 8

            var hasFutureSessions = _unitOfWork.GetRepository<Session>()
                .GetAll(S => sessionIds.Contains(S.Id) && S.StartDate > DateTime.Now).Any();

            if (hasFutureSessions) return false;

            var MemberShips = _unitOfWork.GetRepository<Membership>().GetAll(X => X.MemberId == MemberId);

            try
            {
                if (MemberShips.Any())
                {
                    foreach (var membership in MemberShips)
                        _unitOfWork.GetRepository<Membership>().Delete(membership);
                }
                _unitOfWork.GetRepository<Member>().Delete(Member);
                bool IsDeleted = _unitOfWork.SaveChanges() > 0;
                if (IsDeleted)
                    _attachmentService.Delete(Member.Photo, "members");
                return IsDeleted;
            }
            catch
            {
                return false;
            }

        }
        public bool UpdateMemberDetails(int Id, MemberToUpdateViewModel UpdatedMember)
        {
            var emailExist = _unitOfWork.GetRepository<Member>().GetAll(
                m => m.Email == UpdatedMember.Email && m.Id != Id);

            var PhoneExist = _unitOfWork.GetRepository<Member>().GetAll(
                m => m.Phone == UpdatedMember.Phone && m.Id != Id);

            if (emailExist.Any() || PhoneExist.Any()) return false;

            var Repo = _unitOfWork.GetRepository<Member>();
            var Member = Repo.GetById(Id);
            if (Member is null) return false;
            _mapper.Map(UpdatedMember, Member);

            Repo.Update(Member);
            return _unitOfWork.SaveChanges() > 0;
        }

        #region Helper Methods

        private bool IsEmailExists(string email)
        {
            var existing = _unitOfWork.GetRepository<Member>().GetAll(
                m => m.Email == email);
            return existing.Any();
        }
        private bool IsPhoneExists(string phone)
        {
            var existing = _unitOfWork.GetRepository<Member>().GetAll(
                m => m.Phone == phone);
            return existing.Any();
        }
        #endregion
    }
}
