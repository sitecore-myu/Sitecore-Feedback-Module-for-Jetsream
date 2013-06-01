
namespace Sitecore.Feedback.Module.PresentationLayer.ControlPanel
{
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Pages;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.WebControls;
  using System;
  using System.Globalization;
  using System.Text.RegularExpressions;
  using System.Web.UI.WebControls;
  using Literal = Sitecore.Web.UI.HtmlControls.Literal;

  public class ConfigurationWizardForm : WizardForm
  {
    private readonly int _recipientsMax = int.Parse(Sitecore.Configuration.Settings.GetSetting("Feedback.Recipients.MaxCount"));

    private Edit editProjectName;
    private Memo memoSubject;
    private GridPanel gpConfigurations;
    private Border borderConfigurations;
    private Literal lRecipient1;
    private Web.UI.HtmlControls.Button btnAdd;
    private Web.UI.HtmlControls.Button btnRemove;

    private int _recipients = 1;
    private int Recipients
    {
      get
      {
        _recipients = int.Parse(lRecipient1.Value);
        return _recipients;
      }
      set
      {
        lRecipient1.Value = value.ToString(CultureInfo.InvariantCulture);
        SheerResponse.Refresh(borderConfigurations);
        _recipients = value;
      }
    }

    private Item _configurationItem;
    private Item ConfigurationItem
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

    protected override void OnLoad(System.EventArgs e)
    {
      base.OnLoad(e);
      if (Context.ClientPage.IsEvent) return;
      using (new EditContext(ConfigurationItem))
      {
        editProjectName.Value = ConfigurationItem.Fields["ProjectName"].Value;
        memoSubject.Value = ConfigurationItem.Fields["Subject"].Value;
        var lstEmails = ConfigurationItem.Fields["Recipients"].Value;
        if (lstEmails != null)
        {
          var arrayEmails = lstEmails.Split(',');
          var numberRecipient = 1;
          foreach (var arrayEmail in arrayEmails)
          {
            var editRecipient = gpConfigurations.FindControl("editRecipient" + numberRecipient) as Edit;
            var lRecipient = gpConfigurations.FindControl("lRecipient" + numberRecipient) as Literal;
            if (editRecipient != null && lRecipient != null)
              editRecipient.Value = arrayEmail;
            else
              if (!string.IsNullOrEmpty(arrayEmail))
              {
                CreateControls(numberRecipient, arrayEmail);
                Recipients++;
                btnRemove.Visible = true;
              }
            numberRecipient++;
          }
        }
      }
    }

    private void BtnAddOnClick()
    {
      if (Recipients < _recipientsMax)
      {
        Recipients++;
        CreateControls(Recipients, string.Empty);
        SheerResponse.Refresh(borderConfigurations);
        btnRemove.Visible = true;
        if (Recipients >= _recipientsMax)
          btnAdd.Visible = false;
      }
    }

    private void CreateControls(int id, string email)
    {
      var editRecipient = new Edit
      {
        Width = new Unit("100%"),
        ID = "editRecipient" + id,
        Value = email
      };
      for (var i = 1; i < id; i++)
      {
        var edit = gpConfigurations.FindControl("editRecipient" + i) as Edit;
        if (edit != null) edit.Width = new Unit("100%");
      }
      var lRecipient = new Literal
      {
        Text = "E-mail №" + id,
        ID = "lRecipient" + id,
        Value = Recipients.ToString(CultureInfo.InvariantCulture)
      };
      gpConfigurations.Controls.Add(lRecipient);
      gpConfigurations.Controls.Add(editRecipient);
    }

    private void BtnRemoveOnClick()
    {
      var editRecipient = gpConfigurations.FindControl("editRecipient" + Recipients) as Edit;
      var lRecipient = gpConfigurations.FindControl("lRecipient" + Recipients) as Literal;
      if (editRecipient != null && lRecipient != null)
      {
        gpConfigurations.Controls.Remove(editRecipient);
        gpConfigurations.Controls.Remove(lRecipient);
        SheerResponse.Refresh(borderConfigurations);

        Recipients--;
        if (Recipients <= 1)
          btnRemove.Visible = false;
        else
          btnAdd.Visible = true;
      }
    }

    public static bool IsValidEmailAddress(string email)
    {
      var regex = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$");
      return regex.IsMatch(email);
    }

    protected override void OnNext(object sender, EventArgs formEventArgs)
    {
      var isValidEmails = true;
      for (var i = 1; i <= Recipients; i++)
      {
        var edit = gpConfigurations.FindControl("editRecipient" + i) as Edit;
        if (edit != null && !string.IsNullOrEmpty(edit.Value))
        {
          if (!IsValidEmailAddress(edit.Value))
            isValidEmails = false;
        }
      }
      if (!isValidEmails)
      {
        SheerResponse.Alert("Please enter a valid email!");
      }
      else
      {
        using (new EditContext(ConfigurationItem))
        {
          ConfigurationItem.Editing.BeginEdit();
          ConfigurationItem.Fields["ProjectName"].Value = editProjectName.Value;
          ConfigurationItem.Fields["Subject"].Value = memoSubject.Value;

          var lstRecipients = string.Empty;
          for (var i = 1; i <= Recipients; i++)
          {
            var edit = gpConfigurations.FindControl("editRecipient" + i) as Edit;
            if (edit != null && !string.IsNullOrEmpty(edit.Value))
            {
              if (i != Recipients)
                lstRecipients += edit.Value + ",";
              else
                lstRecipients += edit.Value;
            }
          }
          ConfigurationItem.Fields["Recipients"].Value = lstRecipients;
          ConfigurationItem.Editing.EndEdit();
        }
        base.OnNext(sender, formEventArgs);
      }
    }

    protected override void ActivePageChanged(string page, string oldPage)
    {
      this.BackButton.Visible = false;
      this.NextButton.Header = "Save";
      base.ActivePageChanged(page, oldPage);
    }
  }
}