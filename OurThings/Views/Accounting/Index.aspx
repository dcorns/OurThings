<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Accounting
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Accounting</h2>
    <div id="Here"></div><header id="TopDiv"></header><div id="FeeDiv"></div><div id="CashBackDiv"></div><aside id=TotalsDiv></aside><article id="ActDiv"></article><div id="LocDivMain"></div><div id="PitemDiv"></div><div id="NEWACCTDIV"></div><div id="BottomDiv"></div><div id="TDIV"></div><div id="BTNSDIV"></div><div id="divLists"></div><div id="MainDiv"></div><div id="AcctSelDiv"></div><div id="DivQuickAdd"></div><div id=DivDeposit></div>
    
    <script type="text/javascript">
        $.post('/Accounting/StartAccounting');
    </script>
</asp:Content>
