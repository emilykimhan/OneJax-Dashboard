using System.ComponentModel.DataAnnotations;
//Karrie's just a view model for the organizational building page
namespace OneJaxDashboard.Models
{
    public class OrganizationalBuildingViewModel
    {
        public string PageTitle { get; set; } = "Organizational Building";
        public string StaffSurveyDescription { get; set; } = "Track staff satisfaction and professional development activities.";
        public string PlayDescription { get; set; } = "Play content will go here. This could include team building activities, games, or recreational resources for staff engagement.";
        
        public bool ShowStaffSurveyButton { get; set; } = true;
        public bool ShowPlaySection { get; set; } = true;
    }
}