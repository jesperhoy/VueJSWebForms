#  Vue.js ASP.NET Web Forms Helpers

This library (.NET Framework 4.5) contains 3 ASP.NET Web Forms controls to simplify integrating Vue.js and .vue files with ASP.NET Web Forms.

It is based on and includes the [VueFilesToJS library](https://github.com/jesperhoy/VueFilesToJS).

## How to use

[Download the "VueJSWebForms.dll" file](https://github.com/jesperhoy/VueJSWebForms/releases) and drop this into your ASP.NET web site's "bin" folder.

You can reference the assembly in a Web Forms page like this:

```ASP
<%@ Register Assembly="VueJSWebForms" Namespace="VueJSWebForms" TagPrefix="vue" %>
```

And then use the controls with .vue files like this:

```HTML
<vue:Component file="/car.vue" runat="server" />

<vue:App file="/app.vue" runat="server" />
```

Or in-line like this:

```HTML
<vue:Component name="car" runat="server">
    <template>
        <li>This {{make}} is {{(new Date()).getFullYear() - year}} years old.</li>
    </template>
    <script>
        export default {
            props: ['make','year']
        }
    </script>
</vue:Component>        
```

```HTML
<vue:App runat="server" >
    <template>
        <ul>
            <car v-for="car in cars" :make="car.Make" :year="car.Year" />
        </ul>
    <template>
    <script>
        export default {
            data: { cars: [{Make:'Buick',Year:2001},{Make:'Pontiac',Year:1998}] }
        }
    </script>
</vue:App>
```
...or any combination.

The really cool part about in-line Vue components / apps is that the template part can include ASP.NET Web Forms server tags, which are rendered before Vue.js compiles the template. For example:

```HTML
<vue:App runat="server">
    <template>
        <p>Today is: <%:DateTime.Now.ToLongDateString()%></p>
    </template>
    <script>
        export default {
            data: <%=SomeDataFromADatabase%>
        }
    </script>
</vue:Component>        
```

## When to use in-line vs. .vue files?

If you need to include server rendered data inside the Vue template (see above), then in-line is the only option.

Otherwise, the easiest way is to start in-line. Then at some point when the page becomes too big, it is easy to copy the content of each component/app control to separate (.vue) file.


## Web Forms controls 

- **Component**

    Makes it easy to render a Vue.js component for re-use in a Vue.js application or in other Vue.js components.

    Attributes/Properties:
    - `File` - virtual path of a .vue file (for example "/components/list1.vue"). If not specified, the content of the control is used instead (.vue file format).
    - `Name` - the name of the component (tag-name in app/other components). If not specified and `File` is, the file name (without path / suffix) will be used as the component name.
    - `SquashWS` - Boolean value (default true) indicating if all white space in HTML templates should be squashed (sequences of space, `<LF>`, `<CR>`, `<Tab>` are replaced with a single space).

- **App**

    Makes it easy to render a Vue.js application instance.

    Attributes/Properties:
    - `File` - virtual path of a .vue file (for example "/components/list1.vue"). If not specified, the content of the control is used instead (.vue file format).
    - `VarName` - global JavasScript variable name to reference the Vue instance. If omitted, a random value is used.
    - `Mount` - Boolean value (default true) indicating if a `<div>` tag with a random id should be generated and the Vue instance mounted to this.
    - `SquashWS` - Boolean value (default true) indicating if all white space in HTML templates should be squashed (sequences of space, `<LF>`, `<CR>`, `<Tab>` are replaced with a single space).

  
- **ScriptTemplate**

    Used to extract inner HTML content. The content is not rendered directly to the page but can be obtained through .NET methods.

    The purpose of this is to get HTML syntax highlighting and intellisense in the HTML editor in Visual Studio. This is not really specific to Vue.js, but is convenient when authoring more complex Vue.js applications / components. 

    Methods:
    - `Content()` - Returns the content HTML as a string.
    - `ContentJS()` - Returns content HTML encoded as a JavaScript string.

    Attributes/Properties:
    - `SquashWS` - Boolean value (default true) indicating if all white space in HTML templates should be squashed (sequences of space, `<LF>`, `<CR>`, `<Tab>` are replaced with a single space).


## Examples

- [Using Component and App controls](sample-web-site/sample-vuejs.aspx)
- [Using Component control with .vue file](sample-web-site/sample-vue-file.aspx)
- [Using ScriptTemplate control](sample-web-site/sample-scripttemplate.aspx)

## Requirements / limitations for .vue files (including in-lined)

Note: The aim of this library is to use a .vue file format which is compatible with standard Vue.js build tools (WebPack etc.). Not the other way around. This library does NOT support all the standard .vue file features.

The following .vue file layout is supported:

- The .vue file(s) must have one `<template>` section followed by one `<script>` section, and NO `<style>` section (scoped style is not supported).

- The `<script>` section may reference an external JavaScript script file like this:\
 `<script src="file.js"></script>`

 - The script (contained in or referenced by the `<script>` section) must be plain JavaScript. However, the `src` option mentioned above makes it possible to use TypeScript and other languages that compile to plain JavaScript.\
 (TIP: To use TypeScript source files, use TypeScript compiler options  `"noImplicitUseStrict": true` and `"module": "es6"` to make the compiled .js files match these requirements)

- The script may only contain one or more `import` statements (to include other .vue files as components) followed by a single `export default {...}` statement.

- Each script `import` statement must follow the exact format:\
`import Name from 'file.vue';`

An example of a valid .vue file:

```HTML
<template>
    <dog :name="DogName">
</template>

<script>
import dog from 'dog.vue';
export default {
    data: { DogName: "Fido" },
    components: { "dog": dog }
};
</script>
```

## Versioning

This project uses [Semantic Versioning](https://semver.org/).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Contributions

Contributions are most welcome. No contribution is too big or too small.

Fork this repository, clone locally, make your updates, commit, push, create a pull request in GitHub...


