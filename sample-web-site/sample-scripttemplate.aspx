<%@ Page Language="C#" %>
<%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vwf" %>

<html>
<head>
  <script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
</head>

<body>
  <form id="form1" runat="server">


<vwf:ScriptTemplate ID="template1" runat="server">
<div>
  {{ message }}
</div>
</vwf:ScriptTemplate>

<div id="app"></div>

<script>
new Vue({
  el: '#app',
  template: '#template1',
  data: {
    message: 'Hello Vue!'
  }
  })
</script>

  </form>
</body>
</html>
