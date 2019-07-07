Imports System.Web.UI

<ParseChildren>
Public Class ServerTemplate
  Inherits System.Web.UI.WebControls.WebControl

  Public Property SquashWS As Boolean = True

  Private _Content As String

  Public Function Content() As String
    Return _Content
  End Function

  Public Function ContentJS() As String
    Return VueFilesToJS.JSStringEncode(_Content)
  End Function

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    _Content = sw.ToString.Trim()
    If SquashWS Then _Content = VueFilesToJS.SquashWhiteSpace(_Content)
  End Sub

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
