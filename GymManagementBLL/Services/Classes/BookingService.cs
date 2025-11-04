using AutoMapper;
using GymManagementBLL.Services.Interfaces;
using GymManagementBLL.ViewModels.BookingViewModels;
using GymManagementBLL.ViewModels.MembershipViewModels;
using GymManagementBLL.ViewModels.SessionViewModels;
using GymManagementDAL.Entities;
using GymManagementDAL.Repositories.Interfaces;

namespace GymManagementBLL.Services.Classes
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public bool CancelBooking(int MemberId, int SessionId)
        {
            var sessionEntity = RetrieveSessionById(SessionId);
            if (!ValidateSessionForCancellation(sessionEntity))
                return false;

            var bookingToCancel = FindBookingByMemberAndSession(MemberId, SessionId);
            if (bookingToCancel == null)
                return false;

            return RemoveBookingFromRepository(bookingToCancel);
        }

        public bool CreateNewBooking(CreateBookingViewModel createdBooking)
        {
            var sessionEntity = RetrieveSessionById(createdBooking.SessionId);
            if (!ValidateSessionForBooking(sessionEntity))
                return false;

            if (!VerifyMemberHasActiveSubscription(createdBooking.MemberId))
                return false;

            if (!CheckSessionCapacityAvailability(createdBooking.SessionId))
                return false;

            return PersistNewBooking(createdBooking);
        }

        public IEnumerable<SessionViewModel> GetAllSessions()
        {
            var upcomingSessions = FetchUpcomingSessionsWithDetails();

            if (upcomingSessions == null || !upcomingSessions.Any())
                return null!;

            return TransformSessionsToViewModels(upcomingSessions);
        }

        public IEnumerable<MemberForSessionViewModel> GetMembersForUpcomingBySessionId(int sessionId)
        {
            var sessionBookings = RetrieveBookingsBySessionId(sessionId);
            return BuildMemberViewModelsForUpcoming(sessionBookings, sessionId);
        }

        public IEnumerable<MemberForSessionViewModel> GetMembersForOngoingBySessionId(int sessionId)
        {
            var sessionBookings = RetrieveBookingsBySessionId(sessionId);
            return BuildMemberViewModelsForOngoing(sessionBookings, sessionId);
        }

        public IEnumerable<MemberSelectListViewModel> GetMembersForDropDown(int sessionId)
        {
            var bookedMemberIdentifiers = ExtractBookedMemberIds(sessionId);
            var eligibleMembers = FetchUnbookedMembers(bookedMemberIdentifiers);
            return _mapper.Map<IEnumerable<MemberSelectListViewModel>>(eligibleMembers);
        }

        public bool MemberAttended(int MemberId, int SessionId)
        {
            var bookingRecord = FindBookingByMemberAndSession(MemberId, SessionId);

            if (bookingRecord == null)
                return false;

            return MarkBookingAsAttended(bookingRecord);
        }

        // Private helper methods
        private Session RetrieveSessionById(int sessionId)
        {
            return _unitOfWork.SessionRepository.GetById(sessionId);
        }

        private bool ValidateSessionForCancellation(Session session)
        {
            return session != null && session.StartDate > DateTime.Now;
        }

        private bool ValidateSessionForBooking(Session session)
        {
            return session != null && session.StartDate > DateTime.Now;
        }

        private BookingEntity FindBookingByMemberAndSession(int memberId, int sessionId)
        {
            return _unitOfWork.BookingRepository
                .GetAll(booking => booking.SessionId == sessionId && booking.MemberId == memberId)
                .FirstOrDefault();
        }

        private bool RemoveBookingFromRepository(BookingEntity booking)
        {
            try
            {
                _unitOfWork.BookingRepository.Delete(booking);
                return _unitOfWork.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool VerifyMemberHasActiveSubscription(int memberId)
        {
            return _unitOfWork.MembershipRepository
                .GetAll(membership => membership.MemberId == memberId && membership.Status == "Active")
                .Any();
        }

        private bool CheckSessionCapacityAvailability(int sessionId)
        {
            var session = RetrieveSessionById(sessionId);
            var bookedCount = _unitOfWork.SessionRepository.GetCountOfBookedSlots(sessionId);
            var availableCapacity = session.Capacity - bookedCount;

            return availableCapacity > 0;
        }

        private bool PersistNewBooking(CreateBookingViewModel bookingModel)
        {
            try
            {
                var newBooking = ConstructBookingEntity(bookingModel);
                _unitOfWork.BookingRepository.Add(newBooking);
                return _unitOfWork.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        private BookingEntity ConstructBookingEntity(CreateBookingViewModel model)
        {
            return new BookingEntity
            {
                MemberId = model.MemberId,
                SessionId = model.SessionId,
                IsAttended = false
            };
        }

        private IEnumerable<Session> FetchUpcomingSessionsWithDetails()
        {
            return _unitOfWork.SessionRepository
                .GetAllSessionsWithTrainerAndCategory(session => session.EndDate >= DateTime.Now)
                .OrderByDescending(session => session.StartDate);
        }

        private IEnumerable<SessionViewModel> TransformSessionsToViewModels(IEnumerable<Session> sessions)
        {
            var viewModels = _mapper.Map<IEnumerable<SessionViewModel>>(sessions);

            foreach (var viewModel in viewModels)
            {
                viewModel.AvailableSlots = CalculateAvailableSlots(viewModel.Id, viewModel.Capacity);
            }

            return viewModels;
        }

        private int CalculateAvailableSlots(int sessionId, int totalCapacity)
        {
            var bookedSlots = _unitOfWork.SessionRepository.GetCountOfBookedSlots(sessionId);
            return totalCapacity - bookedSlots;
        }

        private IEnumerable<BookingEntity> RetrieveBookingsBySessionId(int sessionId)
        {
            return _unitOfWork.BookingRepository.GetBySessionId(sessionId);
        }

        private IEnumerable<MemberForSessionViewModel> BuildMemberViewModelsForUpcoming(
            IEnumerable<BookingEntity> bookings, int sessionId)
        {
            return bookings.Select(booking => new MemberForSessionViewModel
            {
                MemberId = booking.MemberId,
                SessionId = sessionId,
                MemberName = booking.Member.Name,
                BookingDate = booking.CreatedAt.ToString()
            });
        }

        private IEnumerable<MemberForSessionViewModel> BuildMemberViewModelsForOngoing(
            IEnumerable<BookingEntity> bookings, int sessionId)
        {
            return bookings.Select(booking => new MemberForSessionViewModel
            {
                MemberId = booking.MemberId,
                SessionId = sessionId,
                MemberName = booking.Member.Name,
                BookingDate = booking.CreatedAt.ToString(),
                IsAttended = booking.IsAttended
            });
        }

        private List<int> ExtractBookedMemberIds(int sessionId)
        {
            return _unitOfWork.GetRepository<BookingEntity>()
                .GetAll(booking => booking.SessionId == sessionId)
                .Select(booking => booking.MemberId)
                .ToList();
        }

        private IEnumerable<Member> FetchUnbookedMembers(List<int> bookedMemberIds)
        {
            return _unitOfWork.GetRepository<Member>()
                .GetAll(member => !bookedMemberIds.Contains(member.Id));
        }

        private bool MarkBookingAsAttended(BookingEntity booking)
        {
            try
            {
                booking.IsAttended = true;
                booking.UpdatedAt = DateTime.Now;
                _unitOfWork.GetRepository<BookingEntity>().Update(booking);
                return _unitOfWork.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}