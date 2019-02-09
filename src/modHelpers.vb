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
    Dim x = My.Computer.FileSystem.ReadAllText(ctx.Server.MapPath(file)).Trim

    If Not x.EndsWith("</script>") Then Throw New Exception(".vue file '" & file & "' does not end with </script>")
    x = x.Substring(0, x.Length - 9).Trim
    Dim i = x.LastIndexOf("<script")
    If i < 0 Then Throw New Exception("Start <script> tag not found in .vue file '" & file & "'")
    Dim y = x.Substring(i + 7).Trim
    x = x.Substring(0, i).Trim

    REM get script
    Dim Scrpt As String = Nothing
    Dim ScrptFile As String = Nothing
    If y.StartsWith(">") Then
      Scrpt = y.Substring(1).Trim
      ScrptFile = file
    Else
      If Not y.StartsWith("src=""") Then Throw New Exception("Invalid <script> tag in .vue file '" & file & "' - must be <script> or <script src=""..."">")
      y = y.Substring(5)
      i = y.IndexOf(""""c)
      If i < 0 Then Throw New Exception("Invalid <script> tag in .vue file '" & file & "' - must be <script> or <script src=""..."">")
      ScrptFile = ResolveVP(file, y.Substring(0, i).Trim)
      y = y.Substring(i + 1)
      If y <> ">" Then Throw New Exception("Invalid <script> tag in .vue file '" & file & "' - must be <script> or <script src=""..."">")
      Scrpt = My.Computer.FileSystem.ReadAllText(ctx.Server.MapPath(ScrptFile)).Trim
    End If

    REM validate script
    If Not Scrpt.StartsWith("export ") Then Throw New Exception("Script does not start with 'export default {' in file '" & ScrptFile & "'")
    Scrpt = Scrpt.Substring(7).Trim
    If Not Scrpt.StartsWith("default") Then Throw New Exception("Script does not start with 'export default {' in file '" & ScrptFile & "'")
    Scrpt = Scrpt.Substring(7).Trim
    If Not Scrpt.StartsWith("{") Then Throw New Exception("Script does not start with 'export default {' in file '" & ScrptFile & "'")
    If Scrpt.EndsWith(";") Then Scrpt = Scrpt.Substring(0, Scrpt.Length - 1).Trim

    REM validate remaining = template
    If Not x.StartsWith("<template>") Then Throw New Exception(".vue file '" & file & "' does not start with <template>")
    x = x.Substring(10)
    If Not x.EndsWith("</template>") Then Throw New Exception(".vue file '" & file & "' does not end with </template> (after script is extracted)")
    x = x.Substring(0, x.Length - 11).Trim

    Return New ParseVueFileResult With {.Template = x, .Script = Scrpt}
  End Function

  Private Function ResolveVP(curFileVP As String, p As String) As String
    If p.Contains("//") OrElse p.Contains("\") Then Throw New Exception("Invalid path")
    If p.StartsWith("/") Then Return p
    Dim i = curFileVP.LastIndexOf("/"c)
    If i < 0 Then Return p
    If p.StartsWith("./") Then p = p.Substring(2)
    Return curFileVP.Substring(0, i + 1) & p
  End Function

  Friend Function JSStringEncode(x As String) As String
    Return "'" & (x.Replace("\", "\\").Replace(vbCrLf, "\n").Replace(vbCr, "\n").Replace(vbLf, "\n").Replace(vbTab, "\t").Replace("'", "\'")) & "'"
  End Function

  Friend Structure ParseVueFileResult
    Public Template As String
    Public Script As String
  End Structure


End Module
