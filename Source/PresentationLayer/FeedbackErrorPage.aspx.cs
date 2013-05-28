
namespace Sitecore.Feedback.Module.PresentationLayer
{
  using Microsoft.VisualBasic.Devices;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using Sitecore.Diagnostics;
  using Sitecore.Feedback.Module.BusinessLayer;
  using Sitecore.Feedback.Module.BusinessLayer.Model;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Web;
  using System.Windows.Forms;

  public partial class FeedbackErrorPage : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    private void Page_Init(object sender, EventArgs e)
    {
      if (!Request.Url.AbsolutePath.Contains("FeedbackErrorPage"))
      {
        var ctx = HttpContext.Current;
        var exception = ctx.Server.GetLastError();
        Session["ErrorLog_RequestURL"] = ctx.Request.Url;
        Session["ErrorLog_Message"] = exception.InnerException.Message;
        Session["ErrorLog_Source"] = exception.InnerException.Source;
        Session["ErrorLog_StackTrace"] = exception.InnerException.StackTrace;
        ctx.Server.ClearError();
        Response.Redirect(Constants.FeedbackErrorPageUrl);
      }
    }

    protected void btnSendFeedback_Click(object sender, EventArgs e)
    {
      try
      {
        var htmlTemplate = FillEmailTempalte();
        FeedbackMessage.SendEmail(htmlTemplate, tbEmail.Text);
        mvSendFeedback.ActiveViewIndex = mvSendFeedback.ActiveViewIndex + 1;
      }
      catch (Exception ex)
      {
        Log.Error("Send FeedBack: ", ex, this);
      }
    }

    private string FillEmailTempalte()
    {
      var srHtmlTemplate = new StreamReader(Server.MapPath(Constants.EmailTemplate));
      var htmlTemplate = srHtmlTemplate.ReadToEnd();

      var computerInfo = new ComputerInfo();
      var browser = Request.Browser;
      var feedbackProjectName = Sitecore.Configuration.Settings.GetSetting("Feedback.ProjectName");

      var emailModel = new EmailModel
      {
        ProjectName = feedbackProjectName,
        UserOs = computerInfo.OSFullName,
        ScreenHeight = SystemInformation.VirtualScreen.Height.ToString(CultureInfo.InvariantCulture),
        ScreenWidth = SystemInformation.VirtualScreen.Width.ToString(CultureInfo.InvariantCulture),
        Email = tbEmail.Text,
        Comment = tbComment.Text,
        Browser = browser.Browser,
        BrowserVersion = browser.Version,
        ErrorLogRequestUrl = Session["ErrorLog_RequestURL"].ToString(),
        ErrorLogMessage = Session["ErrorLog_Message"].ToString(),
        ErrorLogSource = Session["ErrorLog_Source"].ToString(),
        ErrorLogStackTrace = Session["ErrorLog_StackTrace"].ToString(),
        LastVisitPagesList = GetLastVisits()
      };

      var dicEmailModel = typeof(EmailModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
          .ToDictionary(p => p.Name, p => (p.GetValue(emailModel, null) ?? string.Empty).ToString());

      foreach (var key in dicEmailModel.Keys)
      {
        htmlTemplate = htmlTemplate.Replace(String.Format("#{0}#", key), dicEmailModel[key]);
      }
      return htmlTemplate;

    }

    private string GetLastVisits()
    {
      var visitedPagesList = string.Empty;
      var visitedPages = new List<VisitPageModel>();
      try
      {
        if (Request.Cookies["feedback_UserVisitPages"] != null)
        {
          var jsonStr = Request.Cookies["feedback_UserVisitPages"].Value;
          var t = HttpUtility.UrlDecode(jsonStr);
          var jsreader = new JsonTextReader(new StringReader(t));
          var json = (JObject)new JsonSerializer().Deserialize(jsreader);
          visitedPages = (from p in json["Pages"]
                          select new VisitPageModel
                          {
                            Id = (int)p["id"],
                            Url = (string)p["url"]
                          }).ToList();
        }
      }
      catch (Exception ex)
      {
        Log.Error("", ex, this);
      }

      if (visitedPages.Count > 0)
      {
        foreach (var page in visitedPages)
        {
          visitedPagesList +=
             "<li>" + page.Url + "</li>";
        }
      }
      return visitedPagesList;
    }
  }
}