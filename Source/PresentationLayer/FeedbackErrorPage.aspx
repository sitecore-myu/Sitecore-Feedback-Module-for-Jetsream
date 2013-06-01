<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FeedbackErrorPage.aspx.cs" Inherits="Sitecore.Feedback.Module.PresentationLayer.FeedbackErrorPage" %>

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
    <title>Jetstream :: Error</title>
    <meta name="author">
    <meta name="viewport" content="width=device-width,initial-scale=1">
    <script type="text/javascript" src="/sitecore modules/Shell/Sitecore Feedback Module/js/jquery-1.9.1.min.js"></script>
    <script type="text/javascript" src="/sitecore modules/Shell/Sitecore Feedback Module/js/jquery.cookie.js"></script>
    <link rel="stylesheet" type="text/css" href="/css/jetstream.css">
</head>
<body class="base">
    <form id="form1" runat="server">
        <div class="page-wrapper">
            <div id="main_0_ctl00_HeadingDiv" class="heading opaque">
                <a href="/" alt="Jetstream Home">
                    <h1 id="logo"><span class="logotext">JetStream Demo</span></h1>
                </a>
            </div>

            <div id="main_0_main_content_0_ContentWrapperDiv" class="content-wrapper drop-shadow clearfix">
                <div class="col12 last-col">
                    <asp:MultiView ID="mvSendFeedback" runat="server" ActiveViewIndex="0">
                        <asp:View ID="vSendFeedback" runat="server">
                            <div class="page-header row9">
                                <h1>Uncritical exception occured</h1>
                                <p>Luckily, we have the <strong><a href='#' class="log_link">logs</a></strong>! If you send them to developers, they will be happy to fix it.</p>
                                <div class="log_message mod teaser hidden error">
                                    <%
                                        Response.Write("<p class='error big'>" + Session["ErrorLog_Message"] + "</p>");
                                        Response.Write("<p class='error'>" + Session["ErrorLog_StackTrace"] + "</p>");
                                    %>
                                </div>
                            </div>
                            <div class="inline-form rounded-form">
                                <%--<asp:ValidationSummary ID="RegisterUserValidation" runat="server" ValidationGroup="vgFeedback" ShowSummary="true" />--%>
                                <div class="control-group">
                                    <label>Email:</label>
                                    <asp:TextBox runat="server" ID="tbEmail"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="EmailRFV" Display="Dynamic" runat="server" ControlToValidate="tbEmail"
                                        ErrorMessage="Email is required." CssClass="validationError" ValidationGroup="vgFeedback"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="EmailREV" runat="server" CssClass="validationError"
                                        ErrorMessage="Please enter a valid email" Display="Dynamic" ValidationExpression="^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$"
                                        ValidationGroup="vgFeedback" ControlToValidate="tbEmail"></asp:RegularExpressionValidator>
                                </div>
                                <div class="control-group">
                                    <label>Comment:</label>
                                    <asp:TextBox class="control-textarea" runat="server" ID="tbComment" Rows="12" TextMode="MultiLine"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rvfComment" Display="Dynamic" runat="server" ControlToValidate="tbComment"
                                        ErrorMessage="Comment is required." CssClass="validationError" ValidationGroup="vgFeedback"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator Display="Dynamic" ControlToValidate="tbComment" ID="revComment" ValidationGroup="vgFeedback"
                                        ValidationExpression="^[\s\S]{1,1000}$" CssClass="validationError" runat="server" ErrorMessage="Maximum 1000 characters allowed."></asp:RegularExpressionValidator>
                                </div>
                                <div class="button-group">
                                    <asp:Button CssClass="primary-grad btn rounded" runat="server" ID="btnSendFeedback" OnClick="btnSendFeedback_Click" Text="Send feedback" ValidationGroup="vgFeedback" />
                                </div>
                            </div>
                        </asp:View>
                        <asp:View ID="vEnd" runat="server">
                            <h1>Thanks for you Feedback!</h1>
                        </asp:View>
                    </asp:MultiView>
                </div>
            </div>
            <div class="footer">
                <div class="clearfix"></div>
                <p class="copyright">
                    ©2013 Jetstream. All Rights Reserved.
          <span class="bullet">•</span>
                    <a id="main_0_ctl01_TermsHL" class="medium-link" href="/About/Terms-And-Conditions.aspx">Terms And Conditions</a>
                </p>
            </div>
        </div>

    </form>
    <script type="text/javascript">
        $(document).ready(function () {
            BrowserVersion();
            $.cookie("screen_width", screen.width, { path: '/' });
            $.cookie("screen_height", screen.height, { path: '/' });
            $(".log_link").click(function () {
                $(".log_message").toggle('slow');
                return false;
            });
        });

        function BrowserVersion() {
            var nAgt = navigator.userAgent;
            var browserName = navigator.appName;
            var fullVersion = '' + parseFloat(navigator.appVersion);
            var nameOffset, verOffset, ix;
            // In Opera, the true version is after "Opera" or after "Version"
            if ((verOffset = nAgt.indexOf("Opera")) != -1) {
                browserName = "Opera";
                fullVersion = nAgt.substring(verOffset + 6);
                if ((verOffset = nAgt.indexOf("Version")) != -1)
                    fullVersion = nAgt.substring(verOffset + 8);
            }
                // In MSIE, the true version is after "MSIE" in userAgent
            else if ((verOffset = nAgt.indexOf("MSIE")) != -1) {
                browserName = "Internet Explorer";
                fullVersion = nAgt.substring(verOffset + 5);
            }
                // In Chrome, the true version is after "Chrome" 
            else if ((verOffset = nAgt.indexOf("Chrome")) != -1) {
                browserName = "Google Chrome";
                fullVersion = nAgt.substring(verOffset + 7);
            }
                // In Safari, the true version is after "Safari" or after "Version" 
            else if ((verOffset = nAgt.indexOf("Safari")) != -1) {
                browserName = "Safari";
                fullVersion = nAgt.substring(verOffset + 7);
                if ((verOffset = nAgt.indexOf("Version")) != -1)
                    fullVersion = nAgt.substring(verOffset + 8);
            }
                // In Firefox, the true version is after "Firefox" 
            else if ((verOffset = nAgt.indexOf("Firefox")) != -1) {
                browserName = "Mozilla Firefox";
                fullVersion = nAgt.substring(verOffset + 8);
            }
                // In most other browsers, "name/version" is at the end of userAgent 
            else if ((nameOffset = nAgt.lastIndexOf(' ') + 1) <
                      (verOffset = nAgt.lastIndexOf('/'))) {
                browserName = nAgt.substring(nameOffset, verOffset);
                fullVersion = nAgt.substring(verOffset + 1);
                if (browserName.toLowerCase() == browserName.toUpperCase()) {
                    browserName = navigator.appName;
                }
            }
            if ((ix = fullVersion.indexOf(";")) != -1)
                fullVersion = fullVersion.substring(0, ix);
            if ((ix = fullVersion.indexOf(" ")) != -1)
                fullVersion = fullVersion.substring(0, ix);

            $.cookie("browser_name", browserName, { path: '/' });
            $.cookie("browser_version", fullVersion, { path: '/' });
        }

    </script>
</body>
</html>
