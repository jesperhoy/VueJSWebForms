Imports System.Web.UI

Friend Module Module1

  Friend Function MakeVueOptions(content As String, prpFile As String, prpOptions As String, prpsquashWS As Boolean) As String
    Dim ctx = System.Web.HttpContext.Current
    If String.IsNullOrEmpty(prpFile) Then
      REM In-line template
      If content.Length = 0 Then Throw New Exception("Template is empty")
      If content.StartsWith("<template>", StringComparison.InvariantCultureIgnoreCase) Then
        If Not String.IsNullOrEmpty(prpOptions) Then Throw New Exception("Control cannot have 'Options' property when content starts with <template>")
        Return "(" & VueFilesToJS.CompileText(content, ctx.Server.MapPath("~/"), ctx.Request.Url.AbsolutePath, prpsquashWS) & ")()"
      Else
        prpOptions = If(prpOptions, "").Trim
        If prpOptions.Length = 0 Then prpOptions = "{}"
        If Not prpOptions.StartsWith("{") OrElse Not prpOptions.EndsWith("}") Then Throw New Exception("Invalid 'Options' property")
        If prpsquashWS Then content = VueFilesToJS.SquashWhiteSpace(content)
        Return "{" &
                  "template:" & VueFilesToJS.JSStringEncode(content) & "," &
                  prpOptions.Substring(1)
      End If
    Else
      REM .vue File
      If content.Length > 0 Then Throw New Exception("Control cannot have content when used with 'File' property")
      If Not String.IsNullOrEmpty(prpOptions) Then Throw New Exception("Control cannot both 'File' and 'Options' properties")
      Return "(" & VueFilesToJS.Compile(ctx.Server.MapPath("~/"), prpFile, prpsquashWS) & ")()"
    End If
  End Function

End Module
