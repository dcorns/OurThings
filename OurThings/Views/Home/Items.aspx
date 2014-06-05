<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Items
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Items</h2>
    
    <div id="Here"><div id="TopDiv"></div><div id="LocDiv"><div id="LocDivMain"></div><div id="SavLocDiv"></div></div><div id="BottomDiv"></div></div><div id="TDIV"></div><div id="BTNSDIV"></div>
    
    <script type="text/javascript">
        $.post('/Home/StartItems');
    </script>
</asp:Content>
