<!DOCTYPE html>
<html>
<head>
    <title>CFChallenge</title>
    <link href="../../Content/Site.css" rel="stylesheet" type="text/css" />
    <link href="../../Content/themes/base/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <script src="../../CFResources/jquery-1.11.0.js" type="text/javascript" />
    <script src="../../CFResources/json2.js" type="text/javascript" />
    <script src="../../CFResources/underscore.js" type="text/javascript" />
    <script src="../../CFResources/backbone.js" type="text/javascript" />
    <script src="../../CFResources/spine.js" type="text/javascript" />
    <script src="../../CFResources/ember.js" type="text/javascript" />
    <script src="../../CFResources/handlebars-v1.2.1.js" type="text/javascript" />
    <script src="../../CFResources/CFChallenge.js" type="text/javascript" />
    
</head>

<body>
   <script type="text/x-handlebars">
   <div>
   <ul>
   <li>{{#link-to 'adduser'}}AddUser{{/link-to}}</li>
   <li>{{#link-to 'deluser'}}DeleteUser{{/link-to}}</li>
   <li>{{#link-to 'edituser'}}EditUser{{/link-to}}</li>
   </ul>
   </div>
   {{outlet}}
   </script>
   <script type="text/x-handlebars" id='adduser'>
   <p>ADD USER</p>
   </script>
   <script type="text/x-handlebars" id='edituser'>
   <p>EDIT USER</p>
   </script>
   <script type="text/x-handlebars" id='deluser'>
   <p>DELETE USER</p>
   </script>
 </body>   
    
</html>