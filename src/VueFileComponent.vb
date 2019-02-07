Imports System.Web
Imports System.Web.UI

Public Class FileComponent
  Inherits System.Web.UI.WebControls.WebControl

  REM Using 'Property' rather than 'Dim' - enables Visual Studio HTML editor intellisense
  Property File As String
  Property VueLight As Boolean = False
  Property RenderTemplate As Boolean = False

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    Dim ctx = HttpContext.Current
    Dim Tmpl = My.Computer.FileSystem.ReadAllText(ctx.Server.MapPath(File)).Trim
    Dim i = Tmpl.IndexOf("<script>")
    If i < 0 Then Throw New Exception("Start script tag not found")
    Dim j = Tmpl.IndexOf("</script>", i)
    If j < 0 Then Throw New Exception("Closing script tag not found")
    Dim Scrpt = Tmpl.Substring(i + 8, j - i - 8).Trim
    Tmpl = (Tmpl.Substring(0, i) & Tmpl.Substring(j + 9)).Trim

    REM validate script
    If Not Scrpt.StartsWith("export ") Then Throw New Exception("Script does not start with 'export default {'")
    Scrpt = Scrpt.Substring(7).Trim
    If Not Scrpt.StartsWith("default") Then Throw New Exception("Script does not start with 'export default {'")
    Scrpt = Scrpt.Substring(7).Trim
    If Not Scrpt.StartsWith("{") Then Throw New Exception("Script does not start with 'export default {'")
    If Scrpt.EndsWith(";") Then Scrpt = Scrpt.Substring(0, Scrpt.Length - 1).Trim

    REM validate remaining template
    If Not Tmpl.StartsWith("<template>") Then Throw New Exception("File does not start with <template> after script is extracted")
    Tmpl = Tmpl.Substring(10)
    If Not Tmpl.EndsWith("</template>") Then Throw New Exception("File does not end with </template> after script is extracted")
    Tmpl = Tmpl.Substring(0, Tmpl.Length - 11).Trim

    If VueLight Then RenderVueLight(Tmpl, writer) Else RenderVueJS(Tmpl, Scrpt, writer)
  End Sub

  Private Sub RenderVueJS(x As String, scrpt As String, writer As HtmlTextWriter)
    Dim tp As String
    If RenderTemplate Then
      writer.WriteLine("<script type=""x-template"" id=""" & ID & "_template"">")
      writer.Write(x)
      writer.WriteLine("</script>")
      tp = "#" & ID & "_template"
    Else
      REM minimize whitepace
      x = x.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ").Replace(vbTab, " ")
      While x.IndexOf("  ") > 0
        x = x.Replace("  ", " ")
      End While
      tp = x.Replace("\", "\\").Replace("'", "\'")
    End If

    writer.WriteLine("<script>")
    writer.WriteLine("Vue.component('" & ID & "', {" & vbCrLf &
           "  template: '" & tp & "'," & vbCrLf &
           scrpt.Substring(1).Trim & ");")
    writer.WriteLine("</script>")
  End Sub

  Private Sub RenderVueLight(x As String, writer As HtmlTextWriter)
    If RenderTemplate Then
      writer.WriteLine("<script type=""x-template"">")
      writer.Write(x)
      writer.WriteLine("</script>")
    End If

    Dim co = New CompilerOptions
    Dim lst = GetContextComponentList()
    For Each cmp In lst
      co.Components.Add(cmp, "VLC_" & cmp)
    Next
    lst.Add(ID)
    writer.WriteLine("<script>")
    writer.WriteLine("var VLC_" & ID & "=" & Compiler.Compile(x, co) & ";")
    writer.WriteLine("</script>")
  End Sub

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
