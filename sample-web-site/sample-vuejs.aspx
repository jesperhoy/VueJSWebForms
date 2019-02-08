<%@ Page Language="C#" %>
<%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vue" %>

<%
    VueApp1.DataJS = @"{ title: 'Age of cars',
                          cars: [{ Make: 'Buick', Year:2001 },
                                  { Make: 'Pontiac', Year:1998 },
                                  { Make: 'BMW', Year:2003 },
                                  { Make: 'Nissan', Year:2015 }]}";
%>
<html>
  <head>
     <script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
  </head>

<body>
  <form id="form1" runat="server">

<vue:Component Name="car" props="make,year" runat="server">
  <li>This {{make}} is {{(new Date()).getFullYear() - year}} years old.</li>
</vue:Component>

<vue:App ID="VueApp1" Compile="false" runat="server">
<div>
  <h1>{{title}}</h1>
  <ul>
    <car v-for="car in cars" :make="car.Make" :year="car.Year" />
  </ul>
</div>
</vue:App>

  </form>
</body>
</html>
