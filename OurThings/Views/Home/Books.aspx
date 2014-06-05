<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Books
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Books</h2>

    <div id='Here'></div>
    <div id='TDIV'></div>
    <script type="text/javascript">
        $.post('/Home/StartBooks');

</script>
</asp:Content>
