Imports System.Web.UI

<ParseChildren>
Public Class App
  Inherits System.Web.UI.WebControls.WebControl

  Property File As String
  Property VarName As String
  Property Options As String
  Property Mount As Boolean = True
  Property SquashWS As Boolean = True

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    Dim Content = sw.ToString.Trim

    Dim DivName = "VueApp"
    Dim rnd = New Random
    For i = 0 To 10
      DivName &= Chr(65 + rnd.Next(0, 26))
    Next
    If String.IsNullOrEmpty(VarName) Then VarName = DivName
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

    If Mount Then writer.WriteLine("<div id=""" & DivName & """></div>")
    writer.WriteLine("<script>")
    writer.WriteLine("var " & VarName & "=new Vue((" & f & ")());")
    If Mount Then writer.WriteLine(VarName & ".$mount('#" & DivName & "');")
    writer.WriteLine("</script>")
  End Sub

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
