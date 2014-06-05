<%@ Page Title="" Language="C#"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="../../Content/Site.css" rel="stylesheet" type="text/css" />
    <link href="../../Content/themes/base/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <script src="../../Scripts/jquery-1.4.2.min.js" type="text/javascript"></script>
    <script src="../../Scripts/jquery-ui-1.8.1.custom.min.js" type="text/javascript"></script>
    
</head>

<body>
    <h2>Document</h2>
    <div id="TopDiv"></div><div id="MidDiv"></div><div id="BottomDiv"></div>
</body>

<script type="text/javascript">
    var DocID=<%:ViewData["DocID"]%>;
    $.post('/Accounting/LoadDocument', { DocID:DocID});

</script>

</html>
