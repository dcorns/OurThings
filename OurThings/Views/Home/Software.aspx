<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Software
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Software</h2>
    <div id='Here'></div>
    <div id='TDIV'></div>
    <script type="text/javascript">
        $.post('/Home/StartSoftware');

</script>
</asp:Content>
