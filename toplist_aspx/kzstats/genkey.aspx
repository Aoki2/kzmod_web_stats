<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="genkey.aspx.cs" Inherits="kzstats.genkey" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Map stats</title>
    <style type="text/css">
        .aoki_css
        {
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
            <asp:Label ID="gcKeyLabel" runat="server" Font-Bold="True" Font-Names="Arial" 
                Font-Size="X-Large" ForeColor="#53606F"></asp:Label>
            <br />


            <br />
        </asp:Panel>
    
        <br />
        <asp:Label ID="gcPageLoad" runat="server" Font-Names="Verdana" Font-Size="8pt" 
            ForeColor="#9AA6B4"></asp:Label>
        <br />
            <br />
    
    </div>
    </form>
</body>
</html>
