
namespace Sitecore.Feedback.Module.BusinessLayer.Commands
{
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Text;
  using Sitecore.Web.UI.Sheer;


  public class ConfigurationCommand : Command
  {
    // Methods
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      Context.ClientPage.Start(this, "Process");
    }

    protected void Process(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (args.IsPostBack) return;
      SheerResponse.ShowModalDialog(new UrlString(UIUtil.GetUri("control:ConfigurationWizard")).ToString(), true);
      args.WaitForPostBack();
    }
  }
}