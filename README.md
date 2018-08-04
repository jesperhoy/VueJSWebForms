#  Vue Light .NET Web Forms

This library (.NET Framework 4.5) contains 3 ASP.NET Web Form controls to simplify integrating Vue.js with ASP.NET Web Forms.

Each control can also render using the [Vue Light .NET Compiler](https://github.com/jesperhoy/VueLight) instead of Vue.js by setting the "Compile" attribute to `true` (no reactivity).

## How to use

[Download the "VueLightWebForms.dll" file](https://github.com/jesperhoy/VueLightWebForms/releases) and drop this into your ASP.NET web site's "bin" folder.

You can reference the assembly in a Web Forms page like this:

    <%@ Register Assembly="VueLightWebForms" Namespace="VueLight" TagPrefix="vl" %>

And then instantiate a control (in this case "Component") like this:

    <vl:Component id="thing" params="year,color" runat="server">
        ...Vue.js template...
    </vl:/Component>        

And then use an instance of that component in another Vue.js template:

    <thing year="2018" color="blue" />


## ASP.NET Web Form controls 

- **Render**

    Used to render the results of a Vue.js template using global variables.

    Parameters:
    - `DataJS` - The Vue.js "data" property. A string with a JSON data object (or JavaScript) to use for rendering.
    - `Compile` - If the template should be rendered by the server side compiler.
    - `RenderTemplate` - If the raw template should also be rendered with compiled output (not needed but helpful for debugging if template is generated dynamically)

    Examples of use in a Web Forms page:
    - [Rendered with Vue Light .NET Compiler](sample-web-site/sample1-compiled.aspx)
    - [Rendered with Vue.js](sample-web-site/sample1-vuejs.aspx)

- **Component**

    Makes it easy to render a simple Vue.js component for re-use in Vue.js application or in other Vue.js components.

    Parameters:
    - `ID` - the name of the component.
    - `Props` - a comma separated list of property names for the component.
    - `Compile` - If the template should be rendered by the server side compiler.
    - `RenderTemplate` - If the raw template should also be rendered with compiled output (not needed but helpful for debugging if template is generated dynamically)

    Examples of use in a Web Forms page:
    - [Rendered with Vue Light .NET Compiler](sample-web-site/sample1-compiled.aspx)
    - [Rendered with Vue.js](sample-web-site/sample1-vuejs.aspx)

- **ScriptTemplate**

    Used to render HTML in a `<script type="x-template">` tag.

    The only purpose of this is to get HTML syntax highlighting and intellisense in the HTML editor in Visual Studio. This is not really specific to Vue.js in any way, but is convenient when authoring more complex Vue.js applications / components. 

    Parameters:
    - `ID` - will be rendered as the ID for the script tag.


    [Example of use in a Web Forms page](sample-web-site/sample-scripttemplate.aspx)


Common for all the controls above is that their content is used as the Vue.js template - for example:

    <vl:Render runat="server">
        <ul v-show="z">
            <li v-for="x in y">{{x}}</li>
        </ul>
    </vl:Renter>


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Contributions

Contributions are most welcome. No contribution is too big or too small.

Fork this repository, clone locally, make your updates, commit, push, create a pull request in GitHub...


