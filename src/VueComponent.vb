Imports System.Web.UI

<ParseChildren>
Public Class Component
  Inherits System.Web.UI.WebControls.WebControl

  REM Using 'Property' rather than 'Dim' - enables Visual Studio HTML editor intellisense
  Property Name As String = Nothing
  Property VueLight As Boolean = False
  Property RenderTemplate As Boolean = False
  Property File As String
  Property Props As String 'Comma separated list of props

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    If String.IsNullOrEmpty(File) Then
      REM In-line template

      REM if Name is blank - throw
      If String.IsNullOrEmpty(Name) Then Throw New Exception("Missing 'Name' property")

      Dim sw = New IO.StringWriter
      Dim htw = New HtmlTextWriter(sw)
      MyBase.RenderChildren(htw)
      Dim Tmpl = GetVueTemplate(sw.ToString.Trim())
      Dim Scrpt = "{"
      If Not String.IsNullOrEmpty(Props) Then
        Dim pa = Props.Split(","c)
        Scrpt &= " props: ["
        For i = 0 To pa.Length - 1
          If i > 0 Then Scrpt &= ","
          Scrpt &= "'" & pa(i) & "'"
        Next
        Scrpt &= "]"
      End If
      Scrpt &= "}"
      If VueLight Then RenderVueLight(Tmpl, writer) Else RenderVueJS(Tmpl, Scrpt, writer)

    Else
      REM .vue File

      REM if Name is blank - use file name
      If String.IsNullOrEmpty(Name) Then
        Name = File.Substring(File.LastIndexOf("/") + 1)
        Name = Name.Substring(0, Name.IndexOf(".")).ToLower
      End If

      Dim f = ParseVueFile(File)
      If VueLight Then RenderVueLight(f.Template, writer) Else RenderVueJS(f.Template, f.Script, writer)
    End If
  End Sub

  Private Sub RenderVueJS(x As String, scrpt As String, writer As HtmlTextWriter)
    Dim tp As String
    If RenderTemplate Then
      writer.WriteLine("<script type=""x-template"" id=""" & Name & "_template"">")
      writer.Write(x)
      writer.WriteLine("</script>")
      tp = "#" & Name & "_template"
    Else
      REM minimize whitepace
      x = x.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ").Replace(vbTab, " ")
      While x.IndexOf("  ") > 0
        x = x.Replace("  ", " ")
      End While
      tp = x.Replace("\", "\\").Replace("'", "\'")
    End If

    writer.WriteLine("<script>")
    writer.WriteLine("Vue.component('" & Name & "', {" & vbCrLf &
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

    lst.Add(Name)
    writer.WriteLine("<script>")
    writer.WriteLine("var VLC_" & Name & "=" & Compiler.Compile(x, co) & ";")
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

