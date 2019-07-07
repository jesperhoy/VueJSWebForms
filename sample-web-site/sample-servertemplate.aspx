<%@ Page Language="C#" %>
<%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vue" %>

<html>
<head>
  <script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
</head>

<body>

<vue:ServerTemplate ID="template1" runat="server">
<div>
  <p>Today is: <%:DateTime.Now.ToLongDateString()%></p>
  <p>{{ message }}</p>
</div>
</vue:ServerTemplate>

<div id="app"></div>

<script>
  new Vue({
    el: '#app',
    template: <%=template1.ContentJS()%>,
    data: {
      message: 'Hello Vue!'
    }
  });
</script>

</body>
</html>
