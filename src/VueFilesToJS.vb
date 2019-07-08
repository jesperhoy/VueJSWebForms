Public Class VueFilesToJS
  Private Components As New Dictionary(Of String, Component)
  Private WsRoot As String
  Private SquashWS As Boolean
  Private FileReadCallback As Action(Of String) = Nothing

  Public Shared Function Compile(wsRootPath As String, sourceFile As String, Optional squashWS As Boolean = True, Optional rootFileContent As String = Nothing, Optional fileReadCallback As Action(Of String) = Nothing) As String
    If wsRootPath.EndsWith("\") Then wsRootPath = wsRootPath.Substring(0, wsRootPath.Length - 1)
    Dim inst = New VueFilesToJS With {.WsRoot = wsRootPath, .SquashWS = squashWS, .FileReadCallback = fileReadCallback}
    Return inst.ProcRoot(sourceFile, rootFileContent)
  End Function

  Private Sub New()
    REM private constructor so only Compile function can create instance
  End Sub

  Private Function ProcRoot(vueFile As String, FileContent As String) As String
    Dim res = ParseVueFile(vueFile, Nothing, FileContent)

    Dim sb As New System.Text.StringBuilder
    sb.AppendLine("function() {")

    For Each c In Components.Values
      c.Render(sb)
    Next

    sb.AppendLine("  return {" & vbCrLf &
                    "  template: " & JSStringEncode(res.Template) & "," & vbCrLf &
                    res.Script.Substring(1).Trim & ";")
    sb.AppendLine("}")

    Return sb.ToString
  End Function

  'wsroot = "c:\web-sites\tjek" (no ending \)
  Private Function ParseVueFile(vueFile As String, fromComp As Component, Optional fileContent As String = Nothing) As ParseVueFileResult
    Dim cp = ResolvePath(vueFile)
    Dim vfWin = WsRoot & cp.Replace("/", "\") & JustFN(vueFile)
    Dim x As String
    If fileContent Is Nothing Then
      If FileReadCallback IsNot Nothing Then FileReadCallback(vfWin)
      x = My.Computer.FileSystem.ReadAllText(vfWin).Trim
    Else
      x = fileContent.Trim
    End If

    If Not x.EndsWith("</script>") Then Throw New Exception(".vue file '" & vueFile & "' does not end with </script>")
    x = x.Substring(0, x.Length - 9).Trim
    Dim i = x.LastIndexOf("<script")
    If i < 0 Then Throw New Exception("Start <script> tag not found in .vue file '" & vueFile & "'")
    Dim y = x.Substring(i + 7).Trim
    x = x.Substring(0, i).Trim

    REM get script
    Dim Scrpt As String = Nothing
    Dim sfWin As String = Nothing
    If y.StartsWith(">") Then
      Scrpt = y.Substring(1).Trim
      sfWin = vfWin
    Else
      If Not y.StartsWith("src=""") Then Throw New Exception("Invalid <script> tag in .vue file '" & vueFile & "' - must be <script> or <script src=""..."">")
      y = y.Substring(5)
      i = y.IndexOf(""""c)
      If i < 0 Then Throw New Exception("Invalid <script> tag in .vue file '" & vueFile & "' - must be <script> or <script src=""..."">")
      Dim sfWeb = y.Substring(0, i).Trim
      Dim sfPath = ResolvePath(sfWeb, cp)
      sfWin = WsRoot & sfPath.Replace("/", "\") & JustFN(sfWeb)
      y = y.Substring(i + 1)
      If y <> ">" Then Throw New Exception("Invalid <script> tag in .vue file '" & vueFile & "' - must be <script> or <script src=""..."">")
      Scrpt = My.Computer.FileSystem.ReadAllText(sfWin).Trim
    End If

    REM validate script
    'import Home from './components/Home.vue';
    While Scrpt.StartsWith("import ", StringComparison.InvariantCultureIgnoreCase)
      i = Scrpt.IndexOf(";")
      Dim z = Scrpt.Substring(7, i - 7).Trim
      Scrpt = Scrpt.Substring(i + 1).Trim
      i = z.IndexOf(" ")
      Dim CompName = z.Substring(0, i)
      z = z.Substring(i + 1).Trim
      If Not z.StartsWith("from ") Then Throw New Exception("Invalid import statement in file '" & sfWin & "'")
      z = z.Substring(5).Trim
      If Not ((z.StartsWith("'") And z.EndsWith("'")) Or (z.StartsWith("""") And z.EndsWith(""""))) Then Throw New Exception("Invalid import statement in file '" & sfWin & "'")
      z = z.Substring(1, z.Length - 2)
      ProcComponent(CompName, ResolvePath(z, cp) & JustFN(z), fromComp)
    End While

    If Not Scrpt.StartsWith("export ") Then Throw New Exception("Script (after any import statements) does not start with 'export default {' in file '" & sfWin & "'")
    Scrpt = Scrpt.Substring(7).Trim
    If Not Scrpt.StartsWith("default") Then Throw New Exception("Script (after any import statements) does not start with 'export default {' in file '" & sfWin & "'")
    Scrpt = Scrpt.Substring(7).Trim
    If Not Scrpt.StartsWith("{") Then Throw New Exception("Script (after any import statements) does not start with 'export default {' in file '" & sfWin & "'")
    If Scrpt.EndsWith(";") Then Scrpt = Scrpt.Substring(0, Scrpt.Length - 1).Trim

    REM validate remaining = template
    If Not x.StartsWith("<template>") Then Throw New Exception(".vue file '" & vueFile & "' does not start with <template>")
    x = x.Substring(10)
    If Not x.EndsWith("</template>") Then Throw New Exception(".vue file '" & vueFile & "' does not end with </template> (after script is extracted)")
    x = x.Substring(0, x.Length - 11).Trim

    If SquashWS Then x = SquashWhiteSpace(x)

    Return New ParseVueFileResult With {.Template = x, .Script = Scrpt}
  End Function

  Private Shared WhiteSpace As Char() = (" " & vbTab & vbLf & vbCr).ToArray
  Public Shared Function SquashWhiteSpace(x As String) As String
    Dim sb = New System.Text.StringBuilder
    Dim p = 0
    Dim i As Integer
    Do
      i = x.IndexOfAny(WhiteSpace, p)
      If i < 0 Then sb.Append(x.Substring(p)) : Return sb.ToString
      sb.Append(x.Substring(p, i - p))
      sb.Append(" ")
      p = i + 1
      While p < x.Length AndAlso WhiteSpace.Contains(x(p))
        p += 1
      End While
    Loop
  End Function

  REM cp='/',  '/kurt',  '/kurt/viggo'...
  Private Function ResolvePath(fn As String, Optional cp As String = "/") As String
    If fn.Contains("\") Then Throw New Exception("invalid \ character in file name")
    If fn.StartsWith("/") Then fn = fn.Substring(1) : cp = "/"
    Dim i As Integer
    Dim firstRound = True
    While True
      If fn.StartsWith("../") Then fn = fn.Substring(3) : cp = cp.Substring(0, cp.LastIndexOf("/", cp.Length - 2) + 1) : Continue While
      If fn.StartsWith("./") Then fn = fn.Substring(2) : Continue While
      i = fn.IndexOf("/")
      If i < 0 Then Exit While
      If i = 0 Then Throw New Exception("invalid / in path")
      cp &= fn.Substring(0, i + 1)
      fn = fn.Substring(i + 1)
    End While
    Return cp
  End Function

  Private Function JustFN(fn As String) As String
    Dim i = fn.LastIndexOf("/")
    If i < 0 Then Return fn
    Return fn.Substring(i + 1)
  End Function

  Public Shared Function JSStringEncode(x As String) As String
    Return "'" & (x.Replace("\", "\\").Replace(vbCrLf, "\n").Replace(vbCr, "\n").Replace(vbLf, "\n").Replace(vbTab, "\t").Replace("'", "\'")) & "'"
  End Function

  Private Structure ParseVueFileResult
    Public Template As String
    Public Script As String
  End Structure

  Private Sub ProcComponent(compName As String, vueFile As String, parentComp As Component)
    Dim c As Component = Nothing
    If Components.TryGetValue(compName, c) Then
      If String.Compare(vueFile, c.File, True) <> 0 Then Throw New Exception("Two file components ('" & vueFile & "' / '" & c.File & "') referenced with same import name (" & compName & ")")
      If parentComp IsNot Nothing Then parentComp.SubComps.Add(c)
      Exit Sub
    End If
    For Each c In Components.Values
      If String.Compare(vueFile, c.File, True) = 0 Then
        c = New Component With {.Name = compName, .File = vueFile, .AliasFor = c}
        If parentComp IsNot Nothing Then parentComp.SubComps.Add(c)
        Components.Add(compName, c)
        Exit Sub
      End If
    Next
    c = New Component With {.Name = compName, .File = vueFile}
    If parentComp IsNot Nothing Then parentComp.SubComps.Add(c)
    Components.Add(compName, c)
    c.pvfr = ParseVueFile(vueFile, c)
  End Sub

  Private Class Component
    Public Name As String
    Public File As String
    Public AliasFor As Component = Nothing
    Public pvfr As ParseVueFileResult
    Public SubComps As New List(Of Component)
    Public Rendered As Boolean = False

    Public Sub Render(toSB As System.Text.StringBuilder)
      If Rendered Then Exit Sub
      If AliasFor IsNot Nothing Then
        AliasFor.Render(toSB)
        toSB.AppendLine("  var " & Name & "=" & AliasFor.Name & ";")
      Else
        For Each c In SubComps
          c.Render(toSB)
        Next
        toSB.AppendLine("  var " & Name & "={template:" & JSStringEncode(pvfr.Template) & ",")
        toSB.AppendLine(pvfr.Script.Substring(1).Trim & ";")
      End If
      Rendered = True
    End Sub

  End Class

End Class
