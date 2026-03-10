using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//Karrie
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DataEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RecordHistory(string recordType, string dateFilter, DateTime? startDate, DateTime? endDate)
        {
            // Load all records
            var allStaffSurveys = _context.StaffSurveys_22D.ToList();
            var allProfDev = _context.ProfessionalDevelopments.ToList();
            var allMediaPlacements = _context.MediaPlacements_3D.ToList();
            var allWebsiteTraffic = _context.WebsiteTraffic.ToList();
            var allDonorEvents = _context.DonorEvents_19D.Include(d => d.Strategy).ToList();
            var allCommRates = _context.CommunicationRate.ToList();
            var allFeeForServices = _context.FeeForServices_21D.Include(f => f.Strategy).ToList();
            var allIncomeRecords = _context.income_27D.ToList();
            var allBudgetRecords = _context.BudgetTracking_28D.ToList();
            var allSocialMedia = _context.socialMedia_5D.ToList();
            var allMilestones = _context.achieveMile_6D.ToList();
            var allCommunityPerception = _context.Annual_average_7D.ToList();
            var allDemographics = _context.demographics_8D.Include(d => d.Strategy).ToList();
            var allFrameworkPlans = _context.Plan2026_24D.ToList();
            var allBoardMembers = _context.BoardMember_29D.ToList();
            var allBoardMeetings = _context.BoardMeetingAttendance.ToList();
            var allSelfAssessments = _context.selfAssess_31D.ToList();
            var allVolunteerPrograms = _context.volunteerProgram_40D.ToList();

            // Apply filters
            var filteredStaffSurveys = allStaffSurveys;
            var filteredProfDev = allProfDev;
            var filteredMediaPlacements = allMediaPlacements;
            var filteredWebsiteTraffic = allWebsiteTraffic;
            var filteredDonorEvents = allDonorEvents;
            var filteredCommRates = allCommRates;
            var filteredFeeForServices = allFeeForServices;
            var filteredIncomeRecords = allIncomeRecords;
            var filteredBudgetRecords = allBudgetRecords;
            var filteredSocialMedia = allSocialMedia;
            var filteredMilestones = allMilestones;
            var filteredCommunityPerception = allCommunityPerception;
            var filteredDemographics = allDemographics;
            var filteredFrameworkPlans = allFrameworkPlans;
            var filteredBoardMembers = allBoardMembers;
            var filteredBoardMeetings = allBoardMeetings;
            var filteredSelfAssessments = allSelfAssessments;
            var filteredVolunteerPrograms = allVolunteerPrograms;
            
            // Filter by date
            DateTime filterStartDate = DateTime.MinValue;
            DateTime filterEndDate = DateTime.MaxValue;
            
            if (!string.IsNullOrEmpty(dateFilter))
            {
                switch (dateFilter)
                {
                    case "today":
                        filterStartDate = DateTime.Today;
                        filterEndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                        break;
                    case "week":
                        filterStartDate = DateTime.Today.AddDays(-7);
                        filterEndDate = DateTime.Now;
                        break;
                    case "month":
                        filterStartDate = DateTime.Today.AddDays(-30);
                        filterEndDate = DateTime.Now;
                        break;
                    case "custom":
                        if (startDate.HasValue) filterStartDate = startDate.Value;
                        if (endDate.HasValue) filterEndDate = endDate.Value.AddDays(1).AddSeconds(-1);
                        break;
                }
                
                if (dateFilter != "all")
                {
                    filteredStaffSurveys = filteredStaffSurveys
                        .Where(s => s.CreatedDate >= filterStartDate && s.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredProfDev = filteredProfDev
                        .Where(p => p.CreatedDate >= filterStartDate && p.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredMediaPlacements = filteredMediaPlacements
                        .Where(m => m.CreatedDate >= filterStartDate && m.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredWebsiteTraffic = filteredWebsiteTraffic
                        .Where(w => w.CreatedDate >= filterStartDate && w.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredDonorEvents = filteredDonorEvents
                        .Where(d => d.CreatedDate >= filterStartDate && d.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredCommRates = filteredCommRates
                        .Where(c => c.CreatedDate >= filterStartDate && c.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredFeeForServices = filteredFeeForServices
                        .Where(f => f.CreatedDate >= filterStartDate && f.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredIncomeRecords = filteredIncomeRecords
                        .Where(i => i.CreatedDate >= filterStartDate && i.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredBudgetRecords = filteredBudgetRecords
                        .Where(b => b.CreatedDate >= filterStartDate && b.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredSocialMedia = filteredSocialMedia
                        .Where(s => s.CreatedDate >= filterStartDate && s.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredMilestones = filteredMilestones
                        .Where(m => m.CreatedDate >= filterStartDate && m.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredCommunityPerception = filteredCommunityPerception
                        .Where(c => c.CreatedDate >= filterStartDate && c.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredDemographics = filteredDemographics
                        .Where(d => d.CreatedDate >= filterStartDate && d.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredFrameworkPlans = filteredFrameworkPlans
                        .Where(f => f.CreatedDate >= filterStartDate && f.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredBoardMembers = filteredBoardMembers
                        .Where(b => b.CreatedDate >= filterStartDate && b.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredBoardMeetings = filteredBoardMeetings
                        .Where(b => b.CreatedDate >= filterStartDate && b.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredSelfAssessments = filteredSelfAssessments
                        .Where(s => s.CreatedDate >= filterStartDate && s.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredVolunteerPrograms = filteredVolunteerPrograms
                        .Where(v => v.CreatedDate >= filterStartDate && v.CreatedDate <= filterEndDate)
                        .ToList();
                }
            }
            
            // Filter by record type
            if (recordType == "staff-survey")
            {
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "professional-development")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "media-placements")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "website-traffic")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "donor-events")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "comm-rate")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "fee-for-service")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "earned-income")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "budget-tracking")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "social-media")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "milestone")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "community-perception")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "programs-demographics")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "framework-plan")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "board-member")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "board-meeting")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredSelfAssessments = new List<selfAssess_31D>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "self-assessment")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredVolunteerPrograms = new List<volunteerProgram_40D>();
            }
            else if (recordType == "volunteer-program")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
                filteredDonorEvents = new List<DonorEvent_19D>();
                filteredCommRates = new List<Comm_rate20D>();
                filteredFeeForServices = new List<feeForService_21D>();
                filteredIncomeRecords = new List<income_27D>();
                filteredBudgetRecords = new List<BudgetTracking_28D>();
                filteredSocialMedia = new List<socialMedia_5D>();
                filteredMilestones = new List<achieveMile_6D>();
                filteredCommunityPerception = new List<Annual_average_7D>();
                filteredDemographics = new List<demographics_8D>();
                filteredFrameworkPlans = new List<Plan2026_24D>();
                filteredBoardMembers = new List<BoardMemberRecruitment>();
                filteredBoardMeetings = new List<BoardMeetingAttendance>();
                filteredSelfAssessments = new List<selfAssess_31D>();
            }

            // Set ViewBag data
            ViewBag.StaffSurveys = filteredStaffSurveys;
            ViewBag.ProfessionalDevelopments = filteredProfDev;
            ViewBag.MediaPlacements = filteredMediaPlacements;
            ViewBag.WebsiteTraffic = filteredWebsiteTraffic;
            ViewBag.DonorEvents = filteredDonorEvents;
            ViewBag.CommRates = filteredCommRates;
            ViewBag.FeeForServices = filteredFeeForServices;
            ViewBag.IncomeRecords = filteredIncomeRecords;
            ViewBag.BudgetRecords = filteredBudgetRecords;
            ViewBag.SocialMedia = filteredSocialMedia;
            ViewBag.Milestones = filteredMilestones;
            ViewBag.CommunityPerception = filteredCommunityPerception;
            ViewBag.Demographics = filteredDemographics;
            ViewBag.FrameworkPlans = filteredFrameworkPlans;
            ViewBag.BoardMembers = filteredBoardMembers;
            ViewBag.BoardMeetings = filteredBoardMeetings;
            ViewBag.SelfAssessments = filteredSelfAssessments;
            ViewBag.VolunteerPrograms = filteredVolunteerPrograms;
            ViewBag.RecordType = recordType ?? "all";
            ViewBag.DateFilter = dateFilter ?? "all";
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.TotalCount = allStaffSurveys.Count + allProfDev.Count + allMediaPlacements.Count + allWebsiteTraffic.Count + allDonorEvents.Count + allCommRates.Count + allFeeForServices.Count + allIncomeRecords.Count + allBudgetRecords.Count + allSocialMedia.Count + allMilestones.Count + allCommunityPerception.Count + allDemographics.Count + allFrameworkPlans.Count + allBoardMembers.Count + allBoardMeetings.Count + allSelfAssessments.Count + allVolunteerPrograms.Count;
            ViewBag.VisibleCount = filteredStaffSurveys.Count + filteredProfDev.Count + filteredMediaPlacements.Count + filteredWebsiteTraffic.Count + filteredDonorEvents.Count + filteredCommRates.Count + filteredFeeForServices.Count + filteredIncomeRecords.Count + filteredBudgetRecords.Count + filteredSocialMedia.Count + filteredMilestones.Count + filteredCommunityPerception.Count + filteredDemographics.Count + filteredFrameworkPlans.Count + filteredBoardMembers.Count + filteredBoardMeetings.Count + filteredSelfAssessments.Count + filteredVolunteerPrograms.Count;
            
            return View();
        }

        // Delete Programs Demographics
        [HttpPost]
        public IActionResult DeleteDemographics(int id)
        {
            var record = _context.demographics_8D.Find(id);
            if (record != null)
            {
                _context.demographics_8D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Programs Demographics record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Programs Demographics - GET
        [HttpGet]
        public IActionResult EditDemographics(int id)
        {
            var record = _context.demographics_8D.Include(d => d.Strategy).FirstOrDefault(d => d.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            ViewBag.Strategies = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name", record.StrategyId);
            return View(record);
        }

        // Edit Programs Demographics - POST
        [HttpPost]
        public IActionResult EditDemographics(demographics_8D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.demographics_8D.Find(model.Id);
                if (existing != null)
                {
                    existing.StrategyId = model.StrategyId;
                    existing.Year = model.Year;
                    existing.ZipCodes = model.ZipCodes;
                    existing.Notes = model.Notes;
                    _context.SaveChanges();
                    TempData["Success"] = "Programs Demographics record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            ViewBag.Strategies = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name", model.StrategyId);
            return View(model);
        }

        // Delete Community Perception Survey
        [HttpPost]
        public IActionResult DeleteCommunityPerception(int id)
        {
            var record = _context.Annual_average_7D.Find(id);
            if (record != null)
            {
                _context.Annual_average_7D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Community Perception Survey record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Community Perception Survey - GET
        [HttpGet]
        public IActionResult EditCommunityPerception(int id)
        {
            var record = _context.Annual_average_7D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Community Perception Survey - POST
        [HttpPost]
        public IActionResult EditCommunityPerception(Annual_average_7D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.Annual_average_7D.Find(model.Id);
                if (existing != null)
                {
                    existing.Year = model.Year;
                    existing.Percentage = model.Percentage;
                    existing.TotalRespondents = model.TotalRespondents;
                    existing.RespondentsIdentifyingAsTrusted = model.RespondentsIdentifyingAsTrusted;
                    existing.Notes = model.Notes;
                    _context.SaveChanges();
                    TempData["Success"] = "Community Perception Survey record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            return View(model);
        }

        // Delete Milestone Achievement
        [HttpPost]
        public IActionResult DeleteMilestone(int id)
        {
            var record = _context.achieveMile_6D.Find(id);
            if (record != null)
            {
                _context.achieveMile_6D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Milestone Achievement record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Milestone Achievement - GET
        [HttpGet]
        public IActionResult EditMilestone(int id)
        {
            var record = _context.achieveMile_6D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Milestone Achievement - POST
        [HttpPost]
        public IActionResult EditMilestone(achieveMile_6D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.achieveMile_6D.Find(model.Id);
                if (existing != null)
                {
                    existing.Percentage = model.Percentage;
                    existing.achievedReview = model.achievedReview;
                    _context.SaveChanges();
                    TempData["Success"] = "Milestone Achievement record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            return View(model);
        }

        // Delete Annual Budget Tracking
        [HttpPost]
        public IActionResult DeleteBudgetRecord(int id)
        {
            var record = _context.BudgetTracking_28D.Find(id);
            if (record != null)
            {
                _context.BudgetTracking_28D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Annual Budget Tracking record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Annual Budget Tracking - GET
        [HttpGet]
        public IActionResult EditBudgetRecord(int id)
        {
            var record = _context.BudgetTracking_28D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Annual Budget Tracking - POST
        [HttpPost]
        public IActionResult EditBudgetRecord(BudgetTracking_28D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.BudgetTracking_28D.Find(model.Id);
                if (existing != null)
                {
                    existing.Quarter = model.Quarter;
                    existing.Year = model.Year;
                    existing.CommunityPrograms = model.CommunityPrograms;
                    existing.OneYouthPrograms = model.OneYouthPrograms;
                    existing.InterfaithPrograms = model.InterfaithPrograms;
                    existing.HumanitarianEvent = model.HumanitarianEvent;
                    existing.MiscellaneousExpenses = model.MiscellaneousExpenses;
                    existing.CorporateGiving = model.CorporateGiving;
                    existing.IndividualGiving = model.IndividualGiving;
                    existing.GrantsFoundations = model.GrantsFoundations;
                    existing.CommunityEvents = model.CommunityEvents;
                    existing.PeopleCultureWorkshops = model.PeopleCultureWorkshops;
                    existing.MiscellaneousRevenue = model.MiscellaneousRevenue;
                    existing.Notes = model.Notes;
                    _context.SaveChanges();
                    TempData["Success"] = "Annual Budget Tracking record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            return View(model);
        }

        // Delete Social Media Engagement
        [HttpPost]
        public IActionResult DeleteSocialMedia(int id)
        {
            var record = _context.socialMedia_5D.Find(id);
            if (record != null)
            {
                _context.socialMedia_5D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Social Media Engagement record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Social Media Engagement - GET
        [HttpGet]
        public IActionResult EditSocialMedia(int id)
        {
            var record = _context.socialMedia_5D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Social Media Engagement - POST
        [HttpPost]
        public IActionResult EditSocialMedia(socialMedia_5D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.socialMedia_5D.Find(model.Id);
                if (existing != null)
                {
                    existing.Year = model.Year;
                    existing.JulySeptEngagementRate = model.JulySeptEngagementRate;
                    existing.OctDecEngagementRate = model.OctDecEngagementRate;
                    existing.JanMarEngagementRate = model.JanMarEngagementRate;
                    existing.AprilJuneEngagementRate = model.AprilJuneEngagementRate;
                    existing.GoalMet = model.GoalMet;
                    _context.SaveChanges();
                    TempData["Success"] = "Social Media Engagement record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            return View(model);
        }

        // Delete Staff Survey
        [HttpPost]
        public IActionResult DeleteStaffSurvey(int id)
        {
            var survey = _context.StaffSurveys_22D.Find(id);
            if (survey != null)
            {
                _context.StaffSurveys_22D.Remove(survey);
                _context.SaveChanges();
                TempData["Success"] = "Staff Survey record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Delete Professional Development
        [HttpPost]
        public IActionResult DeleteProfessionalDevelopment(int id)
        {
            var profDev = _context.ProfessionalDevelopments.Find(id);
            if (profDev != null)
            {
                _context.ProfessionalDevelopments.Remove(profDev);
                _context.SaveChanges();
                TempData["Success"] = "Professional Development record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Delete Media Placement
        [HttpPost]
        public IActionResult DeleteMediaPlacement(int id)
        {
            var media = _context.MediaPlacements_3D.Find(id);
            if (media != null)
            {
                _context.MediaPlacements_3D.Remove(media);
                _context.SaveChanges();
                TempData["Success"] = "Media Placement record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Delete Fee-For-Service Revenue
        [HttpPost]
        public IActionResult DeleteFeeForService(int id)
        {
            var record = _context.FeeForServices_21D.Find(id);
            if (record != null)
            {
                _context.FeeForServices_21D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Fee-For-Service Revenue record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Fee-For-Service Revenue - GET
        [HttpGet]
        public IActionResult EditFeeForService(int id)
        {
            var record = _context.FeeForServices_21D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            ViewBag.Strategies = _context.Strategies
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();
            return View(record);
        }

        // Edit Fee-For-Service Revenue - POST
        [HttpPost]
        public IActionResult EditFeeForService(feeForService_21D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.FeeForServices_21D.Find(model.Id);
                if (existing != null)
                {
                    var strategy = _context.Strategies.Find(model.StrategyId);
                    existing.ClientName = model.ClientName;
                    existing.StrategyId = model.StrategyId;
                    existing.EventName = strategy?.Name;
                    existing.WorkshopFormat = model.WorkshopFormat;
                    existing.WorkshopLocation = model.WorkshopLocation;
                    existing.WorkshopDate = model.WorkshopDate;
                    existing.EventPartners = model.EventPartners;
                    existing.NumberOfAttendees = model.NumberOfAttendees;
                    existing.ParticipantSatisfactionRating = model.ParticipantSatisfactionRating;
                    existing.PartnerSatisfactionRating = model.PartnerSatisfactionRating;
                    existing.RevenueReceived = model.RevenueReceived;
                    existing.ExpenseReceived = model.ExpenseReceived;
                    existing.Year = model.Year;
                    _context.SaveChanges();
                    TempData["Success"] = "Fee-For-Service Revenue record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            ViewBag.Strategies = _context.Strategies
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();
            return View(model);
        }

        // Delete Earned Income
        [HttpPost]
        public IActionResult DeleteIncomeRecord(int id)
        {
            var record = _context.income_27D.Find(id);
            if (record != null)
            {
                _context.income_27D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Earned Income record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Earned Income - GET
        [HttpGet]
        public IActionResult EditIncomeRecord(int id)
        {
            var record = _context.income_27D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Earned Income - POST
        [HttpPost]
        public IActionResult EditIncomeRecord(income_27D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.income_27D.Find(model.Id);
                if (existing != null)
                {
                    existing.IncomeSource = model.IncomeSource;
                    existing.Amount = model.Amount;
                    existing.Month = model.Month;
                    existing.Notes = model.Notes;
                    _context.SaveChanges();
                    TempData["Success"] = "Earned Income record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            return View(model);
        }

        // Delete Communication Satisfaction
        [HttpPost]
        public IActionResult DeleteCommRate(int id)
        {
            var record = _context.CommunicationRate.Find(id);
            if (record != null)
            {
                _context.CommunicationRate.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Communication Satisfaction record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Communication Satisfaction - GET
        [HttpGet]
        public IActionResult EditCommRate(int id)
        {
            var record = _context.CommunicationRate.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Communication Satisfaction - POST
        [HttpPost]
        public IActionResult EditCommRate(Comm_rate20D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.CommunicationRate.Find(model.Id);
                if (existing != null)
                {
                    existing.Year = model.Year;
                    existing.AverageCommunicationSatisfaction = model.AverageCommunicationSatisfaction;
                    _context.SaveChanges();
                    TempData["Success"] = "Communication Satisfaction record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        // Delete Donor Event
        [HttpPost]
        public IActionResult DeleteDonorEvent(int id)
        {
            var donorEvent = _context.DonorEvents_19D.Find(id);
            if (donorEvent != null)
            {
                _context.DonorEvents_19D.Remove(donorEvent);
                _context.SaveChanges();
                TempData["Success"] = "Donor Event record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Donor Event - GET
        [HttpGet]
        public IActionResult EditDonorEvent(int id)
        {
            var donorEvent = _context.DonorEvents_19D.Find(id);
            if (donorEvent == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            ViewBag.Strategies = _context.Strategies
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == donorEvent.StrategyId
                }).ToList();
            return View(donorEvent);
        }

        // Edit Donor Event - POST
        [HttpPost]
        public IActionResult EditDonorEvent(DonorEvent_19D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.DonorEvents_19D.Find(model.Id);
                if (existing != null)
                {
                    existing.StrategyId = model.StrategyId;
                    existing.NumberOfParticipants = model.NumberOfParticipants;
                    existing.EventSatisfactionRating = model.EventSatisfactionRating;
                    _context.SaveChanges();
                    TempData["Success"] = "Donor Event record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            ViewBag.Strategies = _context.Strategies
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == model.StrategyId
                }).ToList();
            return View(model);
        }

        // Delete Website Traffic
        [HttpPost]
        public IActionResult DeleteWebsiteTraffic(int id)
        {
            var traffic = _context.WebsiteTraffic.Find(id);
            if (traffic != null)
            {
                _context.WebsiteTraffic.Remove(traffic);
                _context.SaveChanges();
                TempData["Success"] = "Website Traffic record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Staff Survey - GET
        [HttpGet]
        public IActionResult EditStaffSurvey(int id)
        {
            var survey = _context.StaffSurveys_22D.Find(id);
            if (survey == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(survey);
        }

        // Edit Staff Survey - POST
        [HttpPost]
        public IActionResult EditStaffSurvey(StaffSurvey_22D survey)
        {
            if (ModelState.IsValid)
            {
                var existingSurvey = _context.StaffSurveys_22D.Find(survey.Id);
                if (existingSurvey != null)
                {
                    existingSurvey.Year = survey.Year;
                    existingSurvey.SatisfactionRate = survey.SatisfactionRate;
                    
                    _context.SaveChanges();
                    TempData["Success"] = "Staff Survey record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(survey);
        }

        // Edit Professional Development - GET
        [HttpGet]
        public IActionResult EditProfessionalDevelopment(int id)
        {
            var profDev = _context.ProfessionalDevelopments.Find(id);
            if (profDev == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(profDev);
        }

        // Edit Professional Development - POST
        [HttpPost]
        public IActionResult EditProfessionalDevelopment(ProfessionalDevelopment profDev)
        {
            if (ModelState.IsValid)
            {
                var existingProfDev = _context.ProfessionalDevelopments.Find(profDev.Id);
                if (existingProfDev != null)
                {
                    existingProfDev.Year = profDev.Year;
                    existingProfDev.Name = profDev.Name;
                    existingProfDev.Activities = profDev.Activities;
                    
                    _context.SaveChanges();
                    TempData["Success"] = "Professional Development record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(profDev);
        }

        // Edit Media Placement - GET
        [HttpGet]
        public IActionResult EditMediaPlacement(int id)
        {
            var media = _context.MediaPlacements_3D.Find(id);
            if (media == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(media);
        }

        // Edit Media Placement - POST
        [HttpPost]
        public IActionResult EditMediaPlacement(MediaPlacements_3D media)
        {
            if (ModelState.IsValid)
            {
                var existingMedia = _context.MediaPlacements_3D.Find(media.Id);
                if (existingMedia != null)
                {
                    existingMedia.January = media.January;
                    existingMedia.February = media.February;
                    existingMedia.March = media.March;
                    existingMedia.April = media.April;
                    existingMedia.May = media.May;
                    existingMedia.June = media.June;
                    existingMedia.July = media.July;
                    existingMedia.August = media.August;
                    existingMedia.September = media.September;
                    existingMedia.October = media.October;
                    existingMedia.November = media.November;
                    existingMedia.December = media.December;
                    
                    _context.SaveChanges();
                    TempData["Success"] = "Media Placement record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(media);
        }

        // Edit Website Traffic - GET
        [HttpGet]
        public IActionResult EditWebsiteTraffic(int id)
        {
            var traffic = _context.WebsiteTraffic.Find(id);
            if (traffic == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(traffic);
        }

        // Edit Website Traffic - POST
        [HttpPost]
        public IActionResult EditWebsiteTraffic(WebsiteTraffic_4D traffic)
        {
            if (ModelState.IsValid)
            {
                var existingTraffic = _context.WebsiteTraffic.Find(traffic.Id);
                if (existingTraffic != null)
                {
                    existingTraffic.Q1_JulySeptember = traffic.Q1_JulySeptember;
                    existingTraffic.Q2_OctoberDecember = traffic.Q2_OctoberDecember;
                    existingTraffic.Q3_JanuaryMarch = traffic.Q3_JanuaryMarch;
                    existingTraffic.Q4_AprilJune = traffic.Q4_AprilJune;
                    
                    _context.SaveChanges();
                    TempData["Success"] = "Website Traffic record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(traffic);
        }

        // Delete Framework Development Plan
        [HttpPost]
        public IActionResult DeleteFrameworkPlan(int id)
        {
            var record = _context.Plan2026_24D.Find(id);
            if (record != null)
            {
                _context.Plan2026_24D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Framework Development Plan record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Framework Development Plan - GET
        [HttpGet]
        public IActionResult EditFrameworkPlan(int id)
        {
            var record = _context.Plan2026_24D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Framework Development Plan - POST
        [HttpPost]
        public IActionResult EditFrameworkPlan(Plan2026_24D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.Plan2026_24D.Find(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Year = model.Year;
                    existing.Quarter = model.Quarter;
                    existing.FrameworkStatus = model.FrameworkStatus;
                    existing.Notes = model.Notes;
                    existing.GoalMet = model.GoalMet;
                    existing.IssueName = model.IssueName;
                    existing.CrisisDescription = model.CrisisDescription;
                    existing.IssueHandled = model.IssueHandled;
                    _context.SaveChanges();
                    TempData["Success"] = "Framework Development Plan record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("RecordHistory");
                }
            }
            return View(model);
        }

        // Delete Board Member Recruitment
        [HttpPost]
        public IActionResult DeleteBoardMember(int id)
        {
            var record = _context.BoardMember_29D.Find(id);
            if (record != null)
            {
                _context.BoardMember_29D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Board Member Recruitment record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Board Member Recruitment - GET
        [HttpGet]
        public IActionResult EditBoardMember(int id)
        {
            var record = _context.BoardMember_29D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        // Edit Board Member Recruitment - POST
        [HttpPost]
        public IActionResult EditBoardMember(BoardMemberRecruitment model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.BoardMember_29D.Find(model.Id);
                if (existing != null)
                {
                    existing.Year = model.Year;
                    existing.Quarter = model.Quarter;
                    existing.NumberRecruited = model.NumberRecruited;
                    existing.MemberNames = model.MemberNames;
                    existing.TotalProspectOutreach = model.TotalProspectOutreach;
                    existing.ProspectNames = model.ProspectNames;
                    _context.SaveChanges();
                    TempData["Success"] = "Board Member Recruitment record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteBoardMeetingAttendance(int id)
        {
            var record = _context.BoardMeetingAttendance.Find(id);
            if (record != null)
            {
                _context.BoardMeetingAttendance.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Board Meeting Attendance record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        [HttpGet]
        public IActionResult EditBoardMeetingAttendance(int id)
        {
            var record = _context.BoardMeetingAttendance.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        [HttpPost]
        public IActionResult EditBoardMeetingAttendance(BoardMeetingAttendance model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.BoardMeetingAttendance.Find(model.Id);
                if (existing != null)
                {
                    existing.MeetingDate = model.MeetingDate;
                    existing.MembersInAttendance = model.MembersInAttendance;
                    existing.TotalBoardMembers = model.TotalBoardMembers;
                    _context.SaveChanges();
                    TempData["Success"] = "Board Meeting Attendance record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteSelfAssessment(int id)
        {
            var record = _context.selfAssess_31D.Find(id);
            if (record != null)
            {
                _context.selfAssess_31D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Board Self-Assessment record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        [HttpGet]
        public IActionResult EditSelfAssessment(int id)
        {
            var record = _context.selfAssess_31D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        [HttpPost]
        public IActionResult EditSelfAssessment(selfAssess_31D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.selfAssess_31D.Find(model.Id);
                if (existing != null)
                {
                    existing.Year = model.Year;
                    existing.SelfAssessmentScore = model.SelfAssessmentScore;
                    _context.SaveChanges();
                    TempData["Success"] = "Board Self-Assessment record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteVolunteerProgram(int id)
        {
            var record = _context.volunteerProgram_40D.Find(id);
            if (record != null)
            {
                _context.volunteerProgram_40D.Remove(record);
                _context.SaveChanges();
                TempData["Success"] = "Volunteer Program record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        [HttpGet]
        public IActionResult EditVolunteerProgram(int id)
        {
            var record = _context.volunteerProgram_40D.Find(id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(record);
        }

        [HttpPost]
        public IActionResult EditVolunteerProgram(volunteerProgram_40D model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.volunteerProgram_40D.Find(model.Id);
                if (existing != null)
                {
                    existing.Quarter = model.Quarter;
                    existing.Year = model.Year;
                    existing.NumberOfVolunteers = model.NumberOfVolunteers;
                    existing.CommunicationsActivities = model.CommunicationsActivities;
                    existing.RecognitionActivities = model.RecognitionActivities;
                    existing.VolunteerLedInitiatives = model.VolunteerLedInitiatives;
                    existing.InitiativeDescriptions = model.InitiativeDescriptions;
                    _context.SaveChanges();
                    TempData["Success"] = "Volunteer Program record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }
    }
}