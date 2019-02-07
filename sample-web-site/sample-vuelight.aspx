<%@ Page Language="C#" %>
<%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vwf" %>

<%
    vlRender1.DataJS = @"{
  title: 'Age of cars',
  cars: [{ Make: 'Buick', Year:2001 },
          { Make: 'Pontiac', Year:1998 },
          { Make: 'BMW', Year:2003 },
          { Make: 'Nissan', Year:2015 }]}";
%>
<html>
<body>
  <form id="form1" runat="server">

<vwf:Component ID="car" props="make,year" VueLight="true" runat="server">
  <li>This {{make}} is {{(new Date()).getFullYear() - year}} years old.</li>
</vwf:Component>

<vwf:Render ID="vlRender1" VueLight="true" runat="server">
<div>
  <h1>{{title}}</h1>
  <ul>
    <car v-for="car in cars" :make="car.Make" :year="car.Year" />
  </ul>
</div>
</vwf:Render>

  </form>
</body>
</html>
