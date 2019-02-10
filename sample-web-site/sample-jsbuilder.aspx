<%@ Page Language="C#" %>
<html>
  <head>
     <script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
  </head>
  <body>

<div id="app"></div> 

<script>
  var MyData = {
        title: "Age of cars",
        cars: [{ Make: "Buick", Year: 2001 },
        { Make: "Pontiac", Year: 1998 },
        { Make: "BMW", Year: 2003 },
        { Make: "Nissan", Year: 2015 }]
  };

<%  var b = new VueJSWebForms.JSBuilder();
    b.AddVueFileComponent("/car.vue");
    b.AddApp("MyData",
             "<div><h1>{{title}}</h1><ul><car v-for=\"car in cars\" :make=\"car.Make\" :year=\"car.Year\" /></ul></div>");
    var Script = b.ToString();
    Response.Write(Script); %>
</script>

  </body>
</html>
