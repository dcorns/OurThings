<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Locations
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Locations</h2>
    <div id="divLists"></div>
    <div id="LocDIV"></div>
    
    <script type="text/javascript">
        $.post('/Home/StartLocations');
    </script>
</asp:Content>
