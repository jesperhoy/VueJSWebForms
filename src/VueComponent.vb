Imports System.Web.UI

<ParseChildren>
Public Class Component
  Inherits System.Web.UI.WebControls.WebControl

  REM Using 'Property' rather than 'Dim' - enables Visual Studio HTML editor intellisense
  Property Props As String 'Comma separated list of props
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

    writer.WriteLine("<script>")
    writer.Write("Vue.component('" & ID & "', {" & vbCrLf &
                 "    template: '" & tp & "'")
    If Not String.IsNullOrEmpty(Props) Then
      Dim pa = Props.Split(","c)
      x = "["
      For i = 0 To pa.Length - 1
        If i > 0 Then x &= ","
        x &= "'" & pa(i) & "'"
      Next
      x &= "]"
      writer.Write("," & vbCrLf &
             "    props: " & x)
    End If
    writer.WriteLine("  })")
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
    lst.Add(ID)

    writer.WriteLine("<script>")
    writer.WriteLine("var VLC_" & ID & "=" & Compiler.Compile(x, co) & ";")
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

