
namespace Sitecore.Feedback.Module.BusinessLayer.Configuration
{
  using Sitecore.Diagnostics;
  using Sitecore.Install.Framework;
  using Sitecore.Jobs.AsyncUI;
  using System;
  using System.Collections.Specialized;
  using System.IO;
  using System.ServiceModel.Activation;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using System.Web.Hosting;
  using System.Xml;

  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class PostStep : IPostStep
  {
    public void Run(ITaskOutput output, NameValueCollection metaData)
    {
      Assert.ArgumentNotNull(output, "output");
      Assert.ArgumentNotNull(metaData, "metaData");
      try
      {
        Task parent = Task.Factory.StartNew(() =>
        {
          Task.Factory.StartNew(ChangeWebConfig);
        });
        parent.Wait();
        JobContext.SendMessage("sfm:controlpanel");
      }
      catch (Exception ex)
      {
        
        Log.Error("Sitecore Feedback Module Install:",ex,this);
      }    
    }

    private static void ChangeWebConfig()
    {
      try
      {
        var isChangedConfig = false;

        var webConfigFile = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "web.config");
        var fiWebConfig = new FileInfo(webConfigFile);
        Assert.IsNotNull(fiWebConfig, "Can't find webConfigFile.");
        if (fiWebConfig.IsReadOnly)
        {
          File.SetAttributes(webConfigFile, FileAttributes.Normal);
        }
        var webConfigDoc = new XmlDocument();
        webConfigDoc.Load(webConfigFile);

        var nodeErrorsDefault =
          webConfigDoc.SelectSingleNode(string.Format(
            "//configuration/system.web/customErrors[@defaultRedirect='{0}']", Constants.FeedbackErrorPageUrl));
        var nodeErrorsCurrent = webConfigDoc.SelectSingleNode("//configuration/system.web/customErrors");
        if (nodeErrorsDefault == null && nodeErrorsCurrent != null)
        {
          if (nodeErrorsCurrent.Attributes != null)
          {
            if (nodeErrorsCurrent.Attributes["mode"] != null)
              nodeErrorsCurrent.Attributes["mode"].Value = "On";
            else
            {
              var attrMode = webConfigDoc.CreateAttribute("mode");
              attrMode.Value = "On";
              nodeErrorsCurrent.Attributes.Append(attrMode);
            }
            if (nodeErrorsCurrent.Attributes["defaultRedirect"] != null)
              nodeErrorsCurrent.Attributes["defaultRedirect"].Value = Constants.FeedbackErrorPageUrl;
            else
            {
              var attrDefaultRedirect = webConfigDoc.CreateAttribute("defaultRedirect");
              attrDefaultRedirect.Value = Constants.FeedbackErrorPageUrl;
              nodeErrorsCurrent.Attributes.Append(attrDefaultRedirect);
            }
            if (nodeErrorsCurrent.Attributes["redirectMode"] != null)
              nodeErrorsCurrent.Attributes["redirectMode"].Value = "ResponseRewrite";
            else
            {
              var attrRedirectMode = webConfigDoc.CreateAttribute("redirectMode");
              attrRedirectMode.Value = "ResponseRewrite";
              nodeErrorsCurrent.Attributes.Append(attrRedirectMode);
            }
          }
          isChangedConfig = true;
        }

        var nodeConfiguration = webConfigDoc.SelectSingleNode("//configuration");
        var nodeSystemNet = webConfigDoc.SelectSingleNode("//configuration/system.net");
        var nodeSystemNetMailSettings = webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings");
        var nodeSystemNetMailSettingsSmtp = webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings/smtp");
        var nodeSystemNetMailSettingsSmtpNetwork = webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings/smtp/network");
       
        if (nodeConfiguration != null)
        {
          if (nodeSystemNet == null)
          {
            XmlElement elemSystemNet = webConfigDoc.CreateElement("system.net");
            nodeConfiguration.AppendChild(elemSystemNet);
            isChangedConfig = true;
            if (nodeSystemNetMailSettings == null)
            {
              XmlElement elemMailSetting = webConfigDoc.CreateElement("mailSettings");
              elemSystemNet.AppendChild(elemMailSetting);

              if (nodeSystemNetMailSettingsSmtp == null)
              {
                XmlElement elemSmtp = webConfigDoc.CreateElement("smtp");
                elemSmtp.SetAttribute("deliveryMethod", "Network");
                elemSmtp.SetAttribute("from", "");
                elemMailSetting.AppendChild(elemSmtp);

                if (nodeSystemNetMailSettingsSmtpNetwork == null)
                {
                  XmlElement elemNetwork = webConfigDoc.CreateElement("network");
                  elemNetwork.SetAttribute("host", "exchange1ua1.dk.sitecore.net");
                  elemSmtp.AppendChild(elemNetwork);
                }
              }
            }
          }
          else
          {
            if (nodeSystemNetMailSettings == null)
            {
              XmlElement elemMailSetting = webConfigDoc.CreateElement("mailSettings");
              nodeSystemNet.AppendChild(elemMailSetting);
              isChangedConfig = true;
              if (nodeSystemNetMailSettingsSmtp == null)
              {
                XmlElement elemSmtp = webConfigDoc.CreateElement("smtp");
                elemSmtp.SetAttribute("deliveryMethod", "Network");
                elemSmtp.SetAttribute("from", "");
                elemMailSetting.AppendChild(elemSmtp);

                if (nodeSystemNetMailSettingsSmtpNetwork == null)
                {
                  XmlElement elemNetwork = webConfigDoc.CreateElement("network");
                  elemNetwork.SetAttribute("host", "exchange1ua1.dk.sitecore.net");
                  elemSmtp.AppendChild(elemNetwork);
                }
              }
            }
          }
        }

        if (isChangedConfig)
          webConfigDoc.Save(webConfigFile);
      }
      catch (Exception ex)
      {
        Log.Error("Change Web Config ", ex, typeof(Util));
      }
    }
  }

  public static class Util
  {
    public static string StripTagsRegexCompiled(string source)
    {
      return HtmlRegex.Replace(source, string.Empty);
    }
    private static readonly Regex HtmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

  }

}