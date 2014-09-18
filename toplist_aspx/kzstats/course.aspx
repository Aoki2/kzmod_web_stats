<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="course.aspx.cs" Inherits="kzstats.course" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Map stats</title>
    <style type="text/css">
        .aoki_css
        {
        }
        .style1
        {
            width: 242px;
        }
        </style>
</head>
<body runat="server" id = "gcBody" bgcolor="#637385" background="img/bg3.png">
    <form id="form1" runat="server">
    <div style="text-align: center">
    
        <br />
    
        <asp:Panel runat="server" BackColor="#EEEBDC" BorderStyle="Solid" 
            Height="100%" HorizontalAlign="Center" style="margin: 0 auto; padding: 0px"
            Width="740px" Wrap="False" BorderWidth="1px" Font-Bold="False" 
            Font-Names="Verdana" BorderColor="Gray" BackImageUrl="~/img/panel_bg2.png" 
            ID="Panel3">
            <br />
            <asp:Panel ID="Panel1" runat="server" style="text-align: left">
                <asp:Label ID="gcMapName2" runat="server" Font-Bold="False" 
                    Font-Names="Verdana" Font-Size="X-Large" ForeColor="#2E3134">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</asp:Label>
                <asp:HyperLink ID="gcMapLink" runat="server" Font-Names="Arial" 
                    Font-Size="X-Large" ForeColor="#BE0000" Font-Bold="True">[gcMapLink]</asp:HyperLink>
                <asp:Label ID="gcMapName3" runat="server" Font-Bold="True" 
                    Font-Names="Arial" Font-Size="X-Large" ForeColor="#2E3134">:</asp:Label>
                <asp:Label ID="gcCourseName" runat="server" Font-Bold="False" 
                    Font-Names="Arial" Font-Size="X-Large" ForeColor="#2E3134"></asp:Label>
            </asp:Panel>
            <br />
            <table style="width: 100%; height: 25px;">
                <tr>
                    <td class="style1">
                        <asp:DropDownList ID="gcDropDown" runat="server" BackColor="#F4F4F4" 
                            Font-Bold="True" ForeColor="#53606F" Height="22px" style="margin-left: 0px" 
                            Width="180px">
                        </asp:DropDownList>
                    </td>
                    <td>
                        &nbsp;</td>
                    <td>
                        &nbsp;</td>
                </tr>
            </table>
            <asp:PlaceHolder ID="gcLinks" runat="server"></asp:PlaceHolder>
            <asp:Panel ID="Panel4" runat="server" HorizontalAlign="Center" Width="738px" 
                Wrap="False">
            </asp:Panel>
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
