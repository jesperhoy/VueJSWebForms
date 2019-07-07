Imports System.Web.UI

<ParseChildren>
Public Class Component
  Inherits System.Web.UI.WebControls.WebControl

  Property File As String
  Property Name As String
  Property Options As String
  Property SquashWS As Boolean = True

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    If String.IsNullOrEmpty(Name) Then
      If String.IsNullOrEmpty(File) Then Throw New Exception("'Name' or 'File' property is required")
      Name = File.Substring(0, File.LastIndexOf("."))
      Name = Name.Substring(Name.LastIndexOf("/") + 1)
    End If

    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    Dim Content = sw.ToString.Trim

    Dim ctx = System.Web.HttpContext.Current
    Dim f As String

    If String.IsNullOrEmpty(File) Then
      REM In-line template
      If Content.Length = 0 Then Throw New Exception("Template is empty")
      If Content.StartsWith("<template>", StringComparison.InvariantCultureIgnoreCase) Then
        If Not String.IsNullOrEmpty(Options) Then Throw New Exception("Control cannot have 'Options' property when content starts with <template>")
      Else
        If String.IsNullOrEmpty(Options) Then Options = "{}"
        Content = "<template>" & Content & "</template><script>export default " & Options & "</script>"
      End If
      f = VueFilesToJS.CompileText(Content, ctx.Server.MapPath("~/"), ctx.Request.Url.AbsolutePath, SquashWS)
    Else
      REM .vue File
      If Content.Length > 0 Then Throw New Exception("Control cannot have content when used with 'File' property")
      If Not String.IsNullOrEmpty(Options) Then Throw New Exception("Control cannot both 'File' and 'Options' properties")
      f = VueFilesToJS.Compile(ctx.Server.MapPath("~/"), File, SquashWS)
      End If

      writer.WriteLine("<script>")
    writer.WriteLine("Vue.component('" & Name & "',(" & f & ")());")
    writer.WriteLine("</script>")
  End Sub

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
