
namespace Sitecore.Feedback.Module.BusinessLayer.Commands
{
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Text;
  using Sitecore.Web.UI.Sheer;

  public class ControlPanelCommand : Command
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Context.ClientPage.Start(this, "Run");
    }

    protected void Run(ClientPipelineArgs args)
    {
      if (args.IsPostBack) return;
      SheerResponse.ShowModalDialog(new UrlString(UIUtil.GetUri("control:ConfigurationWizard")).ToString(), true);
      args.WaitForPostBack();
    }
  }
}