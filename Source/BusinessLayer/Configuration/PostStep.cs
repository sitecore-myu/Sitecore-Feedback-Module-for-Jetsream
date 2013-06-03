
namespace Sitecore.Feedback.Module.BusinessLayer.Configuration
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
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
  using System.Xml.XPath;

  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class PostStep : IPostStep
  {
    private static Item _configurationItem;
    private static Item ConfigurationItem
    {
      get
      {
        if (_configurationItem == null)
        {
          var masterDb = Sitecore.Configuration.Factory.GetDatabase("master");
          _configurationItem = masterDb.Items[new ID("{204EE63A-1FA6-41C9-9666-7981AC6DBCB3}")];
          return _configurationItem;
        }
        else
        {
          return _configurationItem;
        }
      }
      set { _configurationItem = value; }
    }

    public void Run(ITaskOutput output, NameValueCollection metaData)
    {
      Assert.ArgumentNotNull(output, "output");
      Assert.ArgumentNotNull(metaData, "metaData");
      try
      {
        Task parent = Task.Factory.StartNew(() =>
        {
          Task.Factory.StartNew(ChangeConfigurations);
          Task.Factory.StartNew(ChangeWebConfig);
        });

        parent.Wait();
        JobContext.SendMessage("sfm:controlpanel");
      }
      catch (Exception ex)
      {
        Log.Error("Sitecore Feedback Module Install:", ex, this);
      }
    }

    private static void ChangeConfigurations()
    {
      using (new EditContext(ConfigurationItem))
      {
        ConfigurationItem.Editing.BeginEdit();
        ConfigurationItem.Fields["ProjectName"].Value = "Project name";
        ConfigurationItem.Fields["Subject"].Value = "Subject";
        ConfigurationItem.Fields["Recipients"].Value = "someone@someone.net";
        ConfigurationItem.Editing.EndEdit();
      }
    }

    private static void ChangeWebConfig()
    {
      try
      {

        var webConfigFile = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "web.config");
        var fiWebConfig = new FileInfo(webConfigFile);
        Assert.IsNotNull(fiWebConfig, "Can't find webConfigFile.");
        if (fiWebConfig.IsReadOnly)
        {
          File.SetAttributes(webConfigFile, FileAttributes.Normal);
        }
        var webConfigDoc = new XmlDocument();
        webConfigDoc.Load(webConfigFile);

        CustomErrorPage(webConfigFile, webConfigDoc);
        MailSettings(webConfigFile, webConfigDoc);

        var versionData = GetVersionData();
        var vSitecore = GetMajorVersion(versionData);
        if (string.Equals(vSitecore, "7") && !IsExistNewtonsoftJson(webConfigDoc))
        {
          DependentAssembly(webConfigFile, webConfigDoc);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Change Web Config ", ex, typeof(Util));
      }
    }

    private static bool IsExistNewtonsoftJson(XmlDocument webConfigDoc)
    {
      var nodeRuntime = webConfigDoc.SelectSingleNode("//configuration/runtime");
      if (nodeRuntime != null)
      {
        var nodeAssemblyBinding = nodeRuntime.FirstChild;
        if (nodeAssemblyBinding.Name == "assemblyBinding")
        {
          var lstDependentAssembly = nodeAssemblyBinding.ChildNodes;
          foreach (XmlNode node in lstDependentAssembly)
          {
            var assemblyIdentity = node.FirstChild;
            if (assemblyIdentity != null && assemblyIdentity.Attributes != null)
            {
              var attr = assemblyIdentity.Attributes["name"].Value;
              if (string.Equals(attr, "Newtonsoft.Json"))
                return true;
            }
          }
        }
      }
      return false;
    }

    private static void DependentAssembly(string webConfigFile, XmlDocument webConfigDoc)
    {
      var isChangedConfig = false;

      var nodeRuntime = webConfigDoc.SelectSingleNode("//configuration/runtime");
      if (nodeRuntime != null)
      {
        var nodeAssemblyBinding = nodeRuntime.FirstChild;
        if (nodeAssemblyBinding.Name == "assemblyBinding")
        {
          nodeAssemblyBinding.InnerXml = nodeAssemblyBinding.InnerXml + @"<dependentAssembly><assemblyIdentity name='Newtonsoft.Json' publicKeyToken='30ad4fe6b2a6aeed' /><bindingRedirect oldVersion='0.0.0.0-3.5.0.0' newVersion='4.5.0.0' /></dependentAssembly>";
          isChangedConfig = true;
        }
      }
      if (isChangedConfig)
        webConfigDoc.Save(webConfigFile);
    }

    private static void CustomErrorPage(string webConfigFile, XmlDocument webConfigDoc)
    {
      var isChangedConfig = false;
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

      if (isChangedConfig)
        webConfigDoc.Save(webConfigFile);
    }


    private static void MailSettings(string webConfigFile, XmlDocument webConfigDoc)
    {
      var isChangedConfig = false;
      var nodeConfiguration = webConfigDoc.SelectSingleNode("//configuration");
      var nodeSystemNet = webConfigDoc.SelectSingleNode("//configuration/system.net");
      var nodeSystemNetMailSettings = webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings");
      var nodeSystemNetMailSettingsSmtp = webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings/smtp");
      var nodeSystemNetMailSettingsSmtpNetwork =
        webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings/smtp/network");

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
    private static XPathDocument GetVersionData()
    {
      string versionFilePath = Settings.VersionFilePath;
      try
      {
        return new XPathDocument(versionFilePath);
      }
      catch (Exception exception)
      {
        Log.Error("Error loading Sitecore version information: " + versionFilePath, exception, typeof(About));
      }
      return null;
    }

    private static string GetNodeValue(string xpath, XPathDocument document)
    {
      XPathNavigator xPathNavigator = document.CreateNavigator();
      XPathNavigator xPathNavigator2 = xPathNavigator.SelectSingleNode(xpath);
      if (xPathNavigator2 == null)
      {
        return string.Empty;
      }
      return xPathNavigator2.Value;
    }

    private static string GetMajorVersion(XPathDocument document)
    {
      return GetNodeValue("/*/version/major", document);
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