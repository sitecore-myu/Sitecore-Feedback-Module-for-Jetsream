
namespace Sitecore.Feedback.Module.BusinessLayer.Configuration
{
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines.RenderLayout;
  using Sitecore.Web;
  using System;
  using System.Collections.Specialized;
  using System.IO;
  using System.Web;
  using System.Web.UI;
  using System.Web.UI.HtmlControls;

  public class InjectJs
  {
    public void Process(RenderLayoutArgs args)
    {
      if (Context.Site.Name == "shell")
        return;

      try
      {
        var head = WebUtil.FindControlOfType(Context.Page.Page, typeof(HtmlHead));
        if (head != null)
        {
          var filesJs = Directory.GetFiles(HttpContext.Current.Server.MapPath(Constants.FolderJs));
          foreach (var fileJs in filesJs)
          {
            var extension = Path.GetExtension(fileJs);
            if (extension != null && extension.Contains(".js"))
            {
              var fileName = Path.GetFileNameWithoutExtension(fileJs);
              if (fileName != null && fileName.IndexOf('-') > 1)
                fileName = fileName.Remove(fileName.IndexOf('-'));
              if (!IsFileExistOnPage(head, fileName))
              {
                IncludeJsToControl(head, Constants.FolderJs + Path.GetFileName(fileJs));
              }
            }
          }
        }
      }
      catch (Exception exception)
      {
        Log.Error("Feedback Module, InjectJs: ", exception,this);
      }  
    }

    public Control IncludeJsToControl(Control control, string jsfile)
    {
      var child = new HtmlGenericControl("script");
      child.Attributes.Add("type", "text/javascript");
      child.Attributes.Add("src", jsfile);
      control.Controls.Add(child);
      return control;
    }

    private bool IsFileExistOnPage(Control head, string fileName)
    {
      var pathMainLayout = head.Page.TemplateControl.AppRelativeVirtualPath;
      var fullPathMainLayouth = head.Page.MapPath(pathMainLayout);
      var page = GetPage(fullPathMainLayouth, null);
      return page.Contains(fileName);
    }

    public string GetPage(string url, NameValueCollection headers)
    {
      try
      {
        string ret = string.Empty;
        System.Net.WebRequest myRequest = System.Net.WebRequest.Create(url);
        myRequest.PreAuthenticate = true;
        myRequest.Method = "GET";
        if (headers != null)
          myRequest.Headers.Add(headers);
        System.Net.WebResponse myResponse = myRequest.GetResponse();
        try
        {
          var stream = myResponse.GetResponseStream();
          var streamreader = new System.IO.StreamReader(stream);
          ret = streamreader.ReadToEnd();
          return ret.Replace("\x00", "");
        }
        finally
        {
          myResponse.Close();
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Could not get HTML from " + url + ": " + ex.Message, ex);
      }
    }
  }
}