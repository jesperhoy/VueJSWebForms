<%@ Page Language="C#" %>
<%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vue" %>

<html>
  <head>
     <script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
  </head>
<body>

<vue:Component Name="car" runat="server">
  <template>
    <li>This {{make}} is {{(new Date()).getFullYear() - year}} years old.</li>
  </template>
  <script>
    export default {
      props: ['make','year']
    }
  </script>
</vue:Component>

<vue:App runat="server">
  <template>
    <div>
      <h1>{{title}}</h1>
      <p>Today is: <%:DateTime.Now.ToLongDateString()%></p>
      <ul>
        <car v-for="car in cars" :make="car.Make" :year="car.Year" />
      </ul>
    </div>
  </template>
  <script>
    export default {
      data: { title: 'Age of cars',
              cars: [{ Make: 'Buick', Year:2001 },
                      { Make: 'Pontiac', Year:1998 },
                      { Make: 'BMW', Year:2003 },
                      { Make: 'Nissan', Year:2015 }]}
    }
  </script>
</vue:App>

</body>
</html>
