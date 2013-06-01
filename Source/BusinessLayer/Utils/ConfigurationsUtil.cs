
namespace Sitecore.Feedback.Module.BusinessLayer.Utils
{
  using Sitecore.Data;
  using Sitecore.Data.Items;

  public static class ConfigurationsUtil
  {
    private static Item ConfigurationItem()
    {
      var masterDb = Sitecore.Configuration.Factory.GetDatabase("master");
      var configurationItem = masterDb.Items[new ID(Constants.ConfigurationItemId)];
      return configurationItem;
    }

    public static string GetRecipients()
    {
      var lstEmails = ConfigurationItem().Fields["Recipients"].Value;
      return lstEmails;
    }

    public static string GetSubject()
    {
      var lstEmails = ConfigurationItem().Fields["Subject"].Value;
      return lstEmails;
    }

    public static string GetProjectName()
    {
      var lstEmails = ConfigurationItem().Fields["ProjectName"].Value;
      return lstEmails;
    }
  }
}