<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>


<script runat="server">

    
</script>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <form id="form1" runat="server">

    <h2><%: ViewData["Message"] %>
        
            
    </h2>
    <p>
        
    </p>
    <p>
        &nbsp;</p>
        <div id="menucontainer">
            
                <ul id="menu">              
                    <li><%: Html.ActionLink("BOOKS", "Books", "Home")%></li>
                    <li><%: Html.ActionLink("ITEMS", "Items", "Home")%></li>
                    <li><%: Html.ActionLink("AUDIO", "Audio", "Home")%></li>
                    <li><%: Html.ActionLink("VIDEO", "Video", "Home")%></li>
                    <li><%: Html.ActionLink("KEYS", "Keys", "Home")%></li>
                    <li><%: Html.ActionLink("LOCATIONS", "Locations", "Home")%></li>
                    <li><%: Html.ActionLink("OWNERS", "Owners", "Home")%></li>
                    <li><%: Html.ActionLink("AUTHORS", "Authors", "Home")%></li>
                    <li><%: Html.ActionLink("PUBLISHERS", "Publishers", "Home")%></li>
                    <li><%: Html.ActionLink("SOFTWARE", "Software", "Home")%></li>
                    <li><%: Html.ActionLink("ACCOUNTING", "Index", "Accounting")%></li>
                    <li><%: Html.ActionLink("JOBS", "Index", "Jobs")%></li>
                </ul>
            
            </div>
    </form>
    <div id="BTNDIV"></div>
    <script type="text/javascript">
        $.post('/Common/StartMain');
    </script>
</asp:Content>
