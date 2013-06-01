
namespace Sitecore.Feedback.Module.BusinessLayer
{
  using Sitecore.Diagnostics;
  using Sitecore.Feedback.Module.BusinessLayer.Utils;
  using System;
  using System.Net.Mail;
  using System.Text;

  public static class FeedbackMessage
  {
    public static void SendEmail(string body, string userEmail)
    {
      try
      {
        var message = new MailMessage();
        var fromAddress = new MailAddress(userEmail, userEmail);
        message.From = fromAddress;
        var feedbackRecipients = ConfigurationsUtil.GetRecipients();
        if (feedbackRecipients != null)
        {
          var arrayFeedbackRecipients = feedbackRecipients.Split(',');
          foreach (var recipient in arrayFeedbackRecipients)
          {
            var toAddress = new MailAddress(recipient, recipient);
            message.To.Add(toAddress);
          }
        }
        message.Subject = ConfigurationsUtil.GetSubject();
        message.SubjectEncoding = Encoding.UTF8;
        message.IsBodyHtml = true;
        message.BodyEncoding = Encoding.UTF8;
        message.Body = body;
        MainUtil.SendMail(message);
      }
      catch (Exception ex)
      {
        Log.Error("Send FeedBack Email Message: ", ex, null);
      }
    }
  }
}