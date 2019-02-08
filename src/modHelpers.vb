Module modHelpers

  Friend Function GetContextComponentList() As HashSet(Of String)
    Dim ctx = System.Web.HttpContext.Current
    Dim arr As HashSet(Of String)
    If ctx.Items.Contains("VueLightComponents") Then
      arr = DirectCast(ctx.Items("VueLightComponents"), HashSet(Of String))
    Else
      arr = New HashSet(Of String)
      ctx.Items("VueLightComponents") = arr
    End If
    Return arr
  End Function

  Friend Function ParseVueFile(file As String) As ParseVueFileResult
    Dim ctx = System.Web.HttpContext.Current
    Dim Tmpl = My.Computer.FileSystem.ReadAllText(ctx.Server.MapPath(file)).Trim
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

    Return New ParseVueFileResult With {.Template = Tmpl, .Script = Scrpt}
  End Function

  Friend Structure ParseVueFileResult
    Public Template As String
    Public Script As String
  End Structure


End Module
