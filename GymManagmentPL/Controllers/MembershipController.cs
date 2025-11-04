//using GymManagementBLL.Services.Interfaces;
//using GymManagementBLL.ViewModels.MembershipViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;

//namespace GymManagementPL.Controllers
//{
//	[Authorize]
//	public class MembershipController : Controller
//	{
//		private readonly IMembershipService _membershipService;
//		public MembershipController(IMembershipService membershipService)
//		{
//			_membershipService = membershipService;
//		}
//		public IActionResult Index()
//		{
//			var memberships = _membershipService.GetAllMemberShips();
//			return View(memberships);
//		}
//		public IActionResult Create()
//		{
//			LoadDropdowns();
//			return View();
//		}
//		[HttpPost]
//		public IActionResult Create(CreateMemberShipViewModel model)
//		{
//			if (ModelState.IsValid)
//			{
//				var result = _membershipService.CreateMembership(model);

//				if (result)
//				{
//					TempData["Success"] = "Membership created successfully!";
//					return RedirectToAction(nameof(Index));
//				}
//				else
//				{
//					TempData["Error"] = "Failed to create membership. member have an active membership.";
//				}
//			}
//			LoadDropdowns();
//			return View(model);
//		}
//		[HttpPost]
//		public IActionResult Cancel(int id)
//		{
//			var result = _membershipService.DeleteMemberShip(id);

//			if (result)
//			{
//				TempData["Success"] = "Membership cancelled successfully!";
//			}
//			else
//			{
//				TempData["Error"] = "Failed to cancel membership.";
//			}

//			return RedirectToAction(nameof(Index));
//		}

//		#region Helper Methods
//		private void LoadDropdowns()
//		{
//			var members = _membershipService.GetMembersForDropDown();
//			var plans = _membershipService.GetPlansForDropDown();

//			ViewBag.members = new SelectList(members, "Id", "Name");
//			ViewBag.plans = new SelectList(plans, "Id", "Name");
//		}
//		#endregion

//	}
//}


using GymManagementBLL.Services.Interfaces;
using GymManagementBLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagementPL.Controllers
{
    [Authorize]
    public class MembershipController : Controller
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        public IActionResult Index()
        {
            var activeMemberships = RetrieveAllActiveMemberships();
            return View(activeMemberships);
        }

        public IActionResult Create()
        {
            PrepareDropdownLists();
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateMemberShipViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareDropdownLists();
                return View(model);
            }

            var isCreated = ProcessMembershipCreationRequest(model);
            SetCreationFeedback(isCreated);

            if (isCreated)
                return RedirectToAction(nameof(Index));

            PrepareDropdownLists();
            return View(model);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var isCancelled = ProcessMembershipCancellation(id);
            SetCancellationFeedback(isCancelled);

            return RedirectToAction(nameof(Index));
        }

        // Private helper methods
        private IEnumerable<MemberShipViewModel> RetrieveAllActiveMemberships()
        {
            return _membershipService.GetAllMemberShips();
        }

        private bool ProcessMembershipCreationRequest(CreateMemberShipViewModel model)
        {
            return _membershipService.CreateMembership(model);
        }

        private bool ProcessMembershipCancellation(int memberId)
        {
            return _membershipService.DeleteMemberShip(memberId);
        }

        private void PrepareDropdownLists()
        {
            var availableMembers = FetchMembersForSelection();
            var availablePlans = FetchPlansForSelection();

            ViewBag.members = new SelectList(availableMembers, "Id", "Name");
            ViewBag.plans = new SelectList(availablePlans, "Id", "Name");
        }

        private IEnumerable<MemberSelectListViewModel> FetchMembersForSelection()
        {
            return _membershipService.GetMembersForDropDown();
        }

        private IEnumerable<PlanSelectListViewModel> FetchPlansForSelection()
        {
            return _membershipService.GetPlansForDropDown();
        }

        private void SetCreationFeedback(bool isSuccessful)
        {
            if (isSuccessful)
            {
                TempData["Success"] = "Membership created successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to create membership. member have an active membership.";
            }
        }

        private void SetCancellationFeedback(bool isSuccessful)
        {
            if (isSuccessful)
            {
                TempData["Success"] = "Membership cancelled successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to cancel membership.";
            }
        }
    }
}