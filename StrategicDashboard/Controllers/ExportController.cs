
using Microsoft.AspNetCore.Mvc;
using System.Text;
using ClosedXML.Excel;
using System.IO;
using OneJaxDashboard.Data;
//karrie
public class ExportController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExportController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult ExportStaffSurveyToCsv()
    {
        var surveys = _context.StaffSurveys_22D.ToList();

        var csv = new StringBuilder();
        csv.AppendLine("Id,Name,SatisfactionRate,ProfessionalDevelopmentCount");

        foreach (var s in surveys)
        {
            csv.AppendLine($"{s.Id},{s.Name},{s.SatisfactionRate},{s.ProfessionalDevelopmentCount}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "StaffSurveyData.csv");
    }

    public IActionResult ExportStaffSurveyToExcel()
    {
        var surveys = _context.StaffSurveys_22D.ToList();

        using (var workbook = new ClosedXML.Excel.XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Staff Survey 22D");
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "SatisfactionRate";
            worksheet.Cell(1, 4).Value = "ProfessionalDevelopmentCount";

            for (int i = 0; i < surveys.Count; i++)
            {
                var s = surveys[i];
                worksheet.Cell(i + 2, 1).Value = s.Id;
                worksheet.Cell(i + 2, 2).Value = s.Name;
                worksheet.Cell(i + 2, 3).Value = s.SatisfactionRate;
                worksheet.Cell(i + 2, 4).Value = s.ProfessionalDevelopmentCount;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "StaffSurveyData.xlsx");
            }
        }
    }
}
