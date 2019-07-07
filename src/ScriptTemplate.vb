Imports System.Web.UI

<ParseChildren>
Public Class ScriptTemplate
  Inherits System.Web.UI.WebControls.WebControl

  Public Property SquashWS As Boolean = True

  Private _Content As String

  Public Function Content() As String
    Return _Content
  End Function

  Public Function ContentJS() As String
    Return JSStringEncode(_Content)
  End Function

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    _Content = sw.ToString.Trim()
    If SquashWS Then _Content = SquashWhiteSpace(_Content)
  End Sub

  Private Function JSStringEncode(x As String) As String
    Return "'" & (x.Replace("\", "\\").
    Replace(vbCrLf, "\n").
    Replace(vbCr, "\n").
    Replace(vbLf, "\n").
    Replace(vbTab, "\t").
    Replace("'", "\'").
    Replace("</", "<\/")) & "'"
    '...Replace("</", "<\/") to prevent problems with </script>
  End Function

  Private Shared WhiteSpace As Char() = (" " & vbTab & vbLf & vbCr).ToArray
  Private Function SquashWhiteSpace(x As String) As String
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

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

End Class
