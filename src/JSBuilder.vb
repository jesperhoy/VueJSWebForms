Public Class JSBuilder

  Private VueLight As Boolean = False
  Private sb As New System.Text.StringBuilder
  Private CompLst As New HashSet(Of String)

  Public Sub New()
    REM nothing
  End Sub

  Public Sub New(vueLight As Boolean)
    Me.VueLight = vueLight
  End Sub

  Public Sub AddVueFileComponent(file As String, Optional name As String = Nothing)
    Dim f = ParseVueFile(file)

    REM if Name is blank - use file name
    If String.IsNullOrEmpty(name) Then
      name = file.Substring(file.LastIndexOf("/") + 1)
      name = name.Substring(0, name.IndexOf(".")).ToLower
    End If

    RenderComponent(name, f.Template, f.Script)
  End Sub

  Public Sub AddComponent(name As String, template As String, Optional script As String = "{}")
    RenderComponent(name, template, script)
  End Sub

  Private Sub RenderComponent(name As String, tmpl As String, scrpt As String)
    If VueLight Then
      Dim co = New CompilerOptions
      For Each cmp In CompLst
        co.Components.Add(cmp, "VLC_" & cmp)
      Next
      CompLst.Add(name)
      sb.AppendLine("var VLC_" & name & "=" & Compiler.Compile(tmpl, co) & ";")
    Else 'VueJS
      sb.AppendLine("Vue.component('" & name & "', {" & vbCrLf &
           "  template: " & JSStringEncode(tmpl) & "," & vbCrLf &
           scrpt.Substring(1).Trim & ");")
    End If
  End Sub

  Public Sub AddVueFileApp(file As String, dataJS As String, Optional name As String = "app")
    Dim f = ParseVueFile(file)
    RenderApp(name, dataJS, f.Template, f.Script)
  End Sub

  Public Sub AddApp(dataJS As String, template As String, Optional script As String = "{}", Optional name As String = "app")
    RenderApp(name, dataJS, template, script)
  End Sub

  Private Sub RenderApp(name As String, dataJS As String, tmpl As String, scrpt As String)
    If VueLight Then
      Dim co = New CompilerOptions
      For Each cmp In CompLst
        co.Components.Add(cmp, "VLC_" & cmp)
      Next
      sb.AppendLine("(function() {")
      sb.AppendLine("  var data=" & dataJS & ";")
      sb.AppendLine("  var render=" & Compiler.Compile(tmpl, co) & ";")
      sb.AppendLine("  document.write(render(data));")
      sb.AppendLine("})();")
    Else 'VueJS
      '    writer.WriteLine("<div id=""" & Name & """></div>")
      sb.AppendLine("var " & name & "=new Vue({" & vbCrLf &
             "  el: '#" & name & "'," & vbCrLf &
             "  template: " & JSStringEncode(tmpl) & "," & vbCrLf &
             "  data: " & dataJS & "," & vbCrLf &
             scrpt.Substring(1).Trim & ");")
    End If
  End Sub

  Public Overrides Function ToString() As String
    Return sb.ToString
  End Function

End Class
