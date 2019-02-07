#  Vue.js ASP.NET Web Forms Helpers

This library (.NET Framework 4.5) contains 4 ASP.NET Web Forms controls to simplify integrating Vue.js with ASP.NET Web Forms.

Each control can render for Vue.js or for the [Vue Light .NET Compiler](https://github.com/jesperhoy/VueLight) (no reactivity) by setting the "VueLight" attribute (false/true).

## How to use

[Download the "VueJSWebForms.dll" file](https://github.com/jesperhoy/VueJSWebForms/releases) and drop this into your ASP.NET web site's "bin" folder.

You can reference the assembly in a Web Forms page like this:

    <%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vwf" %>

And then instantiate a control (in this case "Component") like this:

    <vwf:Component id="thing" params="year,color" runat="server">
        ...Vue.js template...
    </vwf:/Component>        

And then use an instance of that component in another Vue.js template:

    <thing year="2018" color="blue" />


## ASP.NET Web Forms controls 

- **Render**

    Used to render the results of a Vue.js template using global variables.

    Parameters:
    - `DataJS` - The Vue.js "data" property. A string with a JSON data object (or JavaScript) to use for rendering.
    - `VueLight` (true/false) - If the template should be compiled and rendered by the Vue Light server side compiler.
    - `RenderTemplate` (true/false) - If the raw template should be rendered (in a `<script type="x-template">` tag). Not technically necessary and makes output larger, but helpful for debugging if template is generated dynamically.

    Examples of use in a Web Forms page:
    - [Rendered with Vue Light .NET Compiler](sample-web-site/sample-vuelight.aspx)
    - [Rendered with Vue.js](sample-web-site/sample-vuejs.aspx)

- **Component**

    Makes it easy to render a simple Vue.js component for re-use in Vue.js application or in other Vue.js components.

    Parameters:
    - `ID` - the name of the component.
    - `Props` - a comma separated list of property names for the component.
    - `VueLight` (true/false) - If the template should be compiled and rendered by the Vue Light server side compiler.
    - `RenderTemplate` (true/false) - If the raw template should be rendered (in a `<script type="x-template">` tag). Not technically necessary and makes output larger, but helpful for debugging if template is generated dynamically.

    Examples of use in a Web Forms page:
    - [Rendered with Vue Light .NET Compiler](sample-web-site/sample-vuelight.aspx)
    - [Rendered with Vue.js](sample-web-site/sample-vuejs.aspx)

- **FileComponent**

    Use to render a Vus.js component from a .vue file for re-use in Vue.js application or in other Vue.js components.

    Parameters:
    - `File` - virtual path of the .vue file (for example "~/components/list1.vue").
    - `ID` - the name of the component.
    - `VueLight` (true/false) - If the template should be compiled and rendered by the Vue Light server side compiler.
    - `RenderTemplate` (true/false) - If the raw template should be rendered (in a `<script type="x-template">` tag). Not technically necessary and makes output larger, but helpful for debugging.

    [Example of use in a Web Forms page](sample-web-site/sample-vue-file.aspx)

    Note: Only the most basic .vue file layout is supported. The .vue file must have exactly one `<template>` section, one `<script>` section, and NO `<style>` section.\
    The `<script>` section must start with `export default {`\
    See [car.vue](sample-web-site/car.vue) as an example.\
    For now, the .vue file `<script>` section will only be included in the rendered JavaScript when the control's 
    `VueLight` attribute set to false (or is not present).
    
- **ScriptTemplate**

    Used to render HTML in a `<script type="x-template">` tag.

    The only purpose of this is to get HTML syntax highlighting and intellisense in the HTML editor in Visual Studio. This is not really specific to Vue.js in any way, but is convenient when authoring more complex Vue.js applications / components. 

    Parameters:
    - `ID` - will be rendered as the ID for the script tag.

    [Example of use in a Web Forms page](sample-web-site/sample-scripttemplate.aspx)


Common for the **Render**, **Component**, and **ScriptTemplate** controls above is that their content is used as the Vue.js template - for example:

    <vl:Render runat="server">
        <ul v-show="z">
            <li v-for="x in y">{{x}}</li>
        </ul>
    </vl:Renter>

The **FileComponent** control does not support any content (template is obtained from the .vue file instead).


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Contributions

Contributions are most welcome. No contribution is too big or too small.

Fork this repository, clone locally, make your updates, commit, push, create a pull request in GitHub...


