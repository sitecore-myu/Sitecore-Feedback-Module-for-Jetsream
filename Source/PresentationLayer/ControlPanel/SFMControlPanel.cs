
namespace Sitecore.Feedback.Module.PresentationLayer.ControlPanel
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Reflection;
  using Sitecore.Resources;
  using Sitecore.Shell.Framework;
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Web;
  using Sitecore.Web.UI;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.XmlControls;
  using System;

  public class SFMControlPanelForm : BaseForm
  {
    protected Scrollbox ControlPanel;
    protected Border Home;
    protected Border Links;
    protected XmlControl LinksSection;
    protected Border Options;
    protected XmlControl OptionsSection;
    protected XmlControl Title;
    protected Literal TitleText;

    public override void HandleMessage(Message message)
    {
      Assert.ArgumentNotNull(message, "message");
      base.HandleMessage(message);
      if (message.Name == "task:goto")
        Context.ClientPage.ClientResponse.SetLocation("/sitecore/shell/applications/SFM Control Panel.aspx?page=" + message["page"]);
      Dispatcher.Dispatch(message);
    }

    protected void HomeClick(object sender, EventArgs e)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(e, "e");
      Context.ClientPage.ClientResponse.SetLocation("/sitecore/shell/applications/SFM Control Panel.aspx");
    }

    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);
      if (!Context.ClientPage.IsEvent)
      {
        string index = WebUtil.GetQueryString("page");
        if (string.IsNullOrEmpty(index))
          index = "/sitecore/content/Applications/SFM Control Panel";
        int num1 = 0;
        int num2 = 0;
        Item obj = Context.Database.Items[index];
        if (obj != null)
        {
          string str1 = obj["Icon"];
          string str2 = obj["Header"];
          if (str2.Length > 0)
          {
            ReflectionUtil.SetProperty(Title, "Icon", str1);
            ReflectionUtil.SetProperty(TitleText, "Text", str2);
          }
          else
          {
            Title.Visible = false;
            ReflectionUtil.SetProperty(LinksSection, "Header", Translate.Text("Pick a category."));
          }
          foreach (Item page in obj.Children)
          {
            if (page.Template.Key == "task option")
            {
              string click = page["Header"];
              string str3 = page["Click"];
              CommandState commandState = CommandState.Enabled;
              Command command = CommandManager.GetCommand(str3);
              if (command != null)
              {
                commandState = CommandManager.QueryState(command, CommandContext.Empty);
                click = command.GetClick(CommandContext.Empty, click);
                str3 = command.GetClick(CommandContext.Empty, str3);
              }
              if (commandState != CommandState.Hidden)
              {
                System.Web.UI.Control child = commandState != CommandState.Disabled ? Resource.GetWebControl("TaskOption") : Resource.GetWebControl("DisabledTaskOption");
                Options.Controls.Add(child);
                ReflectionUtil.SetProperty(child, "Header", click);
                ReflectionUtil.SetProperty(child, "Click", str3);
                ++num2;
              }
            }
            if (page.Template.Key == "task page" && HasChildren(page))
            {
              System.Web.UI.Control webControl = Resource.GetWebControl("TaskPageLink");
              Error.AssertControl(webControl, "TaskPageLink");
              Links.Controls.Add(webControl);
              ReflectionUtil.SetProperty(webControl, "ID", Control.GetUniqueID("id"));
              ReflectionUtil.SetProperty(webControl, "Icon", Images.GetThemedImageSource(page["Icon"], ImageDimension.id48x48));
              ReflectionUtil.SetProperty(webControl, "Header", page["Header"]);
              ReflectionUtil.SetProperty(webControl, "Click", ("task:goto(page=" + page.ID + ")"));
              ++num1;
            }
            if (page.Template.Key == "task link")
            {
              System.Web.UI.Control webControl = Resource.GetWebControl("TaskLink");
              Error.AssertControl(webControl, "TaskLink");
              Links.Controls.Add(webControl);
              ReflectionUtil.SetProperty(webControl, "ID", Control.GetUniqueID("id"));
              ReflectionUtil.SetProperty(webControl, "Icon", page["Icon"]);
              ReflectionUtil.SetProperty(webControl, "Header", page["Header"]);
              ReflectionUtil.SetProperty(webControl, "Click", page["Click"]);
              ++num1;
            }
          }
        }
        if (num2 <= 0)
          OptionsSection.Visible = false;
        if (num1 <= 0)
          LinksSection.Visible = false;
      }
      Home.OnClick += HomeClick;
    }

    private static bool HasChildren(Item page)
    {
      if (!page.HasChildren)
        return false;
      foreach (Item obj in page.Children)
      {
        TemplateItem template = obj.Template;
        if (template == null || template.Key != "task option") continue;
        string name = obj["Click"];
        CommandState commandState = CommandState.Enabled;
        Command command = CommandManager.GetCommand(name);
        if (command != null)
          commandState = CommandManager.QueryState(command, CommandContext.Empty);
        if (commandState != CommandState.Hidden)
          return true;
      }
      return false;
    }
  }
}