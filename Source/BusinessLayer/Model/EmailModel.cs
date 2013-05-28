
namespace Sitecore.Feedback.Module.BusinessLayer.Model
{
  public class EmailModel
  {
    public string ProjectName { get; set; }

    public string UserOs { get; set; }

    public string ScreenHeight { get; set; }

    public string ScreenWidth { get; set; }

    public string Email { get; set; }

    public string Comment { get; set; }

    public string Browser { get; set; }

    public string BrowserVersion { get; set; }

    public string ErrorLogRequestUrl { get; set; }

    public string ErrorLogMessage { get; set; }

    public string ErrorLogSource { get; set; }

    public string ErrorLogStackTrace { get; set; }

    public string LastVisitPagesList { get; set; }
  }
}