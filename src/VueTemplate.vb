Imports System.Web.UI

<ParseChildren>
Public Class Template
  Inherits System.Web.UI.WebControls.WebControl

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    Dim x = GetVueTemplate(sw.ToString.Trim())

    writer.WriteLine("<script type=""x-template""" & If(Not String.IsNullOrEmpty(ID), " id=""" & ID & """", "") & ">")
    writer.Write(x)
    writer.Write("</script>")
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
