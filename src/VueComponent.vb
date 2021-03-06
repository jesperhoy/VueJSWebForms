﻿Imports System.Web.UI

<ParseChildren>
Public Class Component
  Inherits System.Web.UI.WebControls.WebControl
  Implements IPostBackEventHandler

  Property File As String
  Property Name As String
  Property Options As String
  Property SquashWS As Boolean = True

  Public Event PostBack(eventArgument As String)

  Protected Overrides Sub Render(writer As HtmlTextWriter)
    If String.IsNullOrEmpty(Name) Then
      If String.IsNullOrEmpty(File) Then Throw New Exception("'Name' or 'File' property is required")
      Name = File.Substring(0, File.LastIndexOf("."))
      Name = Name.Substring(Name.LastIndexOf("/") + 1)
    End If

    Dim sw = New IO.StringWriter
    Dim htw = New HtmlTextWriter(sw)
    MyBase.RenderChildren(htw)
    Dim Content = sw.ToString.Trim

    Dim opt = MakeVueOptions(Content, File, Options, SquashWS,
                             If(Me.ID Is Nothing, "", "PostBack(ea) {" & Me.Page.ClientScript.GetPostBackEventReference(Me, "#").Replace("'#'", "ea") & "},"))

    writer.WriteLine("<script>")
    writer.WriteLine("Vue.component('" & Name & "'," & opt & ");")
    writer.WriteLine("</script>")
  End Sub

  Public Overrides Sub RenderBeginTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Overrides Sub RenderEndTag(writer As HtmlTextWriter)
    REM nothing
  End Sub

  Public Sub RaisePostBackEvent(eventArgument As String) Implements IPostBackEventHandler.RaisePostBackEvent
    RaiseEvent PostBack(eventArgument)
  End Sub

End Class
