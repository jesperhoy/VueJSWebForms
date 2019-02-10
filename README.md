#  Vue.js ASP.NET Web Forms Helpers

This library (.NET Framework 4.5) contains 3 ASP.NET Web Forms controls and a utility class to simplify integrating Vue.js with ASP.NET Web Forms.

These "helpers" can either render for Vue.js or for the [Vue Light .NET Compiler](https://github.com/jesperhoy/VueLight) (no reactivity).

## How to use

[Download the "VueJSWebForms.dll" file](https://github.com/jesperhoy/VueJSWebForms/releases) and drop this into your ASP.NET web site's "bin" folder.

You can reference the assembly in a Web Forms page like this:

    <%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vue" %>

And then use the controls like this:

    <vue:Component name="car" props="make,year" runat="server">
        <li>This {{make}} is {{(new Date()).getFullYear() - year}} years old.</li>
    </vue:Component>        

    <vue:App datajs="[{Make:'Buick',Year:2001},{Make:'Pontiac',Year:1998}]" runat="server" >
        <ul>
            <car v-for="car in cars" :make="car.Make" :year="car.Year" />
        </ul>
    </vue:App>


You can use the utility class (JSBuilder) like this (C#):

    var b = new VueJSWebForms.JSBuilder();
    b.AddVueFileComponent("/car.vue");
    b.AddVueFileComponent("/plane.vue");
    b.AddVueFileApp("/app.vue","MyData");
    var Script = b.ToString();
    // Optionally cache the Script here...
    Response.Write(Script);



## Web Forms controls 

- **Component**

    Makes it easy to render a Vue.js component for re-use in a Vue.js application or in other Vue.js components.

    Parameters:
    - `File` - virtual path of a .vue file (for example "/components/list1.vue"). If not specified, the content of the control is used as the Vue.js template instead.
    - `Name` - the name of the component (tag-name in app/other components). If not specified and `File` is, the file name (without path / suffix) will be used as the component name.
    - `Props` - a comma separated list of property names for the component (not used when `File` is specified).
    - `RenderTemplate` (true/false) - If the raw template should be rendered (in a `<script type="x-template">` tag). Not technically necessary and makes output larger, but helpful for debugging if template is generated dynamically.
    - `VueLight` (true/false) - If the template should be compiled and rendered by the Vue Light server side compiler (no reactivity).

- **App**

    Used to render the results of a Vue.js template.

    Parameters:
    - `File` - virtual path of a .vue file (for example "/components/list1.vue"). If not specified, the content of the control is used as the Vue.js template instead.
    - `DataJS` - The Vue.js "data" property. A string with a JSON data object (or JavaScript) to use for rendering.
    - `Name` - the name of the Vue.js app variable. Defaults to "app".
    - `RenderTemplate` (true/false) - If the raw template should be rendered (in a `<script type="x-template">` tag). Not technically necessary and makes output larger, but helpful for debugging if template is generated dynamically.
    - `VueLight` (true/false) - If the template should be compiled and rendered by the Vue Light server side compiler (no reactivity).

  
- **Template**

    Used to render HTML in a `<script type="x-template">` tag.

    The only purpose of this is to get HTML syntax highlighting and intellisense in the HTML editor in Visual Studio. This is not really specific to Vue.js in any way, but is convenient when authoring more complex Vue.js applications / components. 

    Parameters:
    - `ID` - will be rendered as the ID for the script tag.


## JSBuilder utility class

This class can be used to build a JavaScript string from multiple .vue files and/or string templates, and then render this to the browser - and possibly caching / saving the result for optimization.

To use the class, instantiate a new instance, use the Add... methods to include Vue.js components / apps, and finally retrieve the JavaScript using the ToString() method.

## Examples

- [Using Component and App controls with Vue.js](sample-web-site/sample-vuejs.aspx)
- [Using Component and App controls with Vue Light .NET Compiler](sample-web-site/sample-vuelight.aspx)
- [Using Component control with .vue file](sample-web-site/sample-vue-file.aspx)
- [Using Template control](sample-web-site/sample-template.aspx)
- [Using JSBuilder utility class](sample-web-site/sample-jsbuilder.aspx)

## Note about .vue files

You can specify a .vue file in the `File` parameter of the **Component** and **App** controls, and as a parameter with the JSBuilder utility class Add... methods.

Only the most basic .vue file layout is supported:
- The .vue file must have exactly one `<template>` section, one `<script>` section, and NO `<style>` section.
- The `<script>` section must start with `export default {`

See [car.vue](sample-web-site/car.vue) as an example.

The .vue file `<script>` section will only be included in the rendered JavaScript when rendering for Vue.js (not Vue Light).


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Contributions

Contributions are most welcome. No contribution is too big or too small.

Fork this repository, clone locally, make your updates, commit, push, create a pull request in GitHub...


