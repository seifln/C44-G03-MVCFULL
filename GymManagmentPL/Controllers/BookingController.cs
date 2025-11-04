using GymManagementBLL.Services.Interfaces;
using GymManagementBLL.ViewModels.BookingViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagementPL.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public IActionResult Index()
        {
            var availableSessions = RetrieveAllSessionSchedules();
            return View(availableSessions);
        }

        public ActionResult GetMembersForUpcomingSession(int id)
        {
            var enrolledMembers = FetchUpcomingSessionParticipants(id);
            return View(enrolledMembers);
        }

        public ActionResult Create(int id)
        {
            PopulateMemberDropdownList(id);
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateBookingViewModel createdBooking)
        {
            var isBookingSuccessful = ProcessNewBookingRequest(createdBooking);
            SetFeedbackMessage(isBookingSuccessful, "Booking Created successfully!", "Failed to Create Booking.");

            return RedirectToAction(nameof(GetMembersForUpcomingSession),
                new { id = createdBooking.SessionId });
        }

        [HttpPost]
        public ActionResult Cancel(int MemberId, int SessionId)
        {
            var isCancellationSuccessful = ProcessBookingCancellation(MemberId, SessionId);
            SetFeedbackMessage(isCancellationSuccessful, "Booking cancelled successfully!", "Failed to cancel Booking.");

            return RedirectToAction(nameof(GetMembersForUpcomingSession), new { id = SessionId });
        }

        public ActionResult GetMembersForOngoingSessions(int id)
        {
            var sessionParticipants = FetchOngoingSessionParticipants(id);
            return View(sessionParticipants);
        }

        [HttpPost]
        public ActionResult Attended(int MemberId, int SessionId)
        {
            ProcessAttendanceRecord(MemberId, SessionId);
            return RedirectToAction(nameof(GetMembersForOngoingSessions), new { id = SessionId });
        }

        // Private helper methods
        private IEnumerable<GymManagementBLL.ViewModels.SessionViewModels.SessionViewModel> RetrieveAllSessionSchedules()
        {
            return _bookingService.GetAllSessions();
        }

        private IEnumerable<MemberForSessionViewModel> FetchUpcomingSessionParticipants(int sessionId)
        {
            return _bookingService.GetMembersForUpcomingBySessionId(sessionId);
        }

        private IEnumerable<MemberForSessionViewModel> FetchOngoingSessionParticipants(int sessionId)
        {
            return _bookingService.GetMembersForOngoingBySessionId(sessionId);
        }

        private void PopulateMemberDropdownList(int sessionId)
        {
            var availableMembers = _bookingService.GetMembersForDropDown(sessionId);
            ViewBag.members = new SelectList(availableMembers, "Id", "Name");
        }

        private bool ProcessNewBookingRequest(CreateBookingViewModel bookingDetails)
        {
            return _bookingService.CreateNewBooking(bookingDetails);
        }

        private bool ProcessBookingCancellation(int memberId, int sessionId)
        {
            return _bookingService.CancelBooking(memberId, sessionId);
        }

        private void ProcessAttendanceRecord(int memberId, int sessionId)
        {
            _bookingService.MemberAttended(memberId, sessionId);
        }

        private void SetFeedbackMessage(bool isSuccessful, string successMessage, string errorMessage)
        {
            if (isSuccessful)
            {
                TempData["SuccessMessage"] = successMessage;
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }
        }
    }
}