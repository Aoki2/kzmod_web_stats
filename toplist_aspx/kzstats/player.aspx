<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="player.aspx.cs" Inherits="kzstats.player" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Map stats</title>
    <style type="text/css">
        .aoki_css
        {
        }
        .style5
        {
            width: 234px;
        }
        .style9
        {
            width: 235px;
        }
        </style>
</head>
<body runat="server" id = "gcBody" bgcolor="#637385" background="img/bg3.png">
    <form id="form1" runat="server">
    <div style="text-align: center">
    
        <br />
    
        <asp:Panel ID="Panel3" runat="server" BackColor="#EEEBDC" BorderStyle="Solid" 
            Height="100%" HorizontalAlign="Center" style="margin: 0 auto; padding: 0px"
            Width="740px" Wrap="False" BorderWidth="1px" Font-Bold="False" 
            Font-Names="Verdana" BorderColor="Gray" BackImageUrl="~/img/panel_bg2.png">
            <br />
            <asp:Label ID="gcLabelName" runat="server" Font-Bold="True" Font-Names="Arial" 
                Font-Size="X-Large" ForeColor="#53606F"></asp:Label>
            <br />
            <asp:Label ID="gcLabelSteamId" runat="server" Font-Bold="False" Font-Names="Arial" 
                Font-Size="Medium" ForeColor="#53606F"></asp:Label>
            <br />
            <table style="width:100%;">
                <tr>
                    <td class="style5">
                        <asp:DropDownList ID="gcDropDown" runat="server" BackColor="#F4F4F4" 
                            Font-Bold="True" ForeColor="#53606F" Height="22px" style="margin-left: 0px" 
                            Width="180px">
                        </asp:DropDownList>
                    </td>
                    <td class="style9">
                        &nbsp;</td>
                    <td>
                        &nbsp;</td>
                </tr>
            </table>
            <asp:PlaceHolder ID="gcLinks" runat="server"></asp:PlaceHolder>
            <asp:Panel ID="Panel4" runat="server">
            </asp:Panel>
            <br />
        </asp:Panel>
    
            <br />
        <br />
        <asp:Label ID="gcPageLoad" runat="server" Font-Names="Verdana" Font-Size="8pt" 
            ForeColor="#9AA6B4"></asp:Label>
        <br />
            <br />
    
    </div>
    </form>
</body>
</html>
