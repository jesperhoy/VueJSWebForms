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

    Dim opt = MakeVueOptions(Content, File, Options, SquashWS)

    If Mount Then
      Dim DivName = "VueApp"
      Dim rnd = New Random
      For i = 0 To 10
        DivName &= Chr(65 + rnd.Next(0, 26))
      Next
      If String.IsNullOrEmpty(VarName) Then VarName = DivName
      writer.WriteLine("<div id=""" & DivName & """></div>")
      writer.WriteLine("<script>")
      writer.WriteLine("var " & VarName & "=new Vue(" & opt & ");")
      writer.WriteLine(VarName & ".$mount('#" & DivName & "');")
      writer.WriteLine("</script>")
    Else
      If String.IsNullOrEmpty(VarName) Then Throw New Exception("Must provide 'VarName' property when 'Mount' is False")
      writer.WriteLine("<script>")
      writer.WriteLine("var " & VarName & "=new Vue(" & opt & ");")
      writer.WriteLine("</script>")
    End If
  End Sub

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
