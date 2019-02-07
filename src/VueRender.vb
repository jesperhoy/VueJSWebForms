Imports System.Web.UI

<ParseChildren>
Public Class Render
  Inherits System.Web.UI.WebControls.WebControl

  Property DataJS As String = "{}"
  Property Compile As Boolean = False
  Property RenderTemplate As Boolean = False

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    Dim x = GetVueTemplate(sw.ToString.Trim())
    If Compile Then RenderCompiled(x, writer) Else RenderVueJS(x, writer)
  End Sub

  Private Sub RenderVueJS(x As String, writer As HtmlTextWriter)
    Dim tp As String
    If RenderTemplate Then
      writer.WriteLine("<script type=""x-template"" id=""" & ID & "_template"">")
      writer.Write(x)
      writer.WriteLine("</script>")
      tp = "#" & ID & "_template"
    Else
      x = x.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ").Replace(vbTab, " ")
      While x.IndexOf("  ") > 0
        x = x.Replace("  ", " ")
      End While
      tp = x.Replace("\", "\\").Replace("'", "\'")
    End If

    writer.WriteLine("<div id=""" & ID & "_replace""></div>")
    writer.WriteLine("<script>")
    writer.WriteLine("new Vue({" & vbCrLf &
                     "    template: '" & tp & "'," &
                     "    el: '#" & ID & "_replace'," &
                     "    data: " & DataJS & "})")
    writer.Write("</script>")
  End Sub

  Private Sub RenderCompiled(x As String, writer As HtmlTextWriter)
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

    writer.WriteLine("<script>")
    writer.WriteLine("(function() {")
    writer.WriteLine("  var data=" & DataJS & ";")
    writer.WriteLine("  var render=" & Compiler.Compile(x, co) & ";")
    writer.WriteLine("  document.write(render(data));")
    writer.WriteLine("})();")
    writer.WriteLine("</script>")
  End Sub

  Protected Overridable Function GetVueTemplate(content As String) As String
    Return content
  End Function

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
