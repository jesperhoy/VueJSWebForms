Imports System.Web.UI

Friend Module Module1

  Friend Function MakeVueOptions(content As String, prpFile As String, prpOptions As String, prpsquashWS As Boolean) As String
    Dim ctx = System.Web.HttpContext.Current
    If String.IsNullOrEmpty(prpFile) Then
      REM In-line template
      If content.Length = 0 Then Throw New Exception("Template is empty")
      If content.StartsWith("<template>", StringComparison.InvariantCultureIgnoreCase) Then
        If Not String.IsNullOrEmpty(prpOptions) Then Throw New Exception("Control cannot have 'Options' property when content starts with <template>")
        Return "(" & VueFilesToJS.Compile(ctx.Server.MapPath("~/"), ctx.Request.Url.AbsolutePath, prpsquashWS, content) & ")()"
      Else
        prpOptions = If(prpOptions, "").Trim
        If prpOptions.Length = 0 Then prpOptions = "{}"
        If Not prpOptions.StartsWith("{") OrElse Not prpOptions.EndsWith("}") Then Throw New Exception("Invalid 'Options' property")
        If prpsquashWS Then content = VueFilesToJS.SquashWhiteSpace(content)
        Return "{" &
                  "template:" & VueFilesToJS.JSStringEncode(content) & "," &
                  prpOptions.Substring(1)
      End If
    End If

    REM .vue File
    If content.Length > 0 Then Throw New Exception("Control cannot have content when used with 'File' property")
    If Not String.IsNullOrEmpty(prpOptions) Then Throw New Exception("Control cannot both 'File' and 'Options' properties")

    'If Not useCache Then Return "(" & VueFilesToJS.Compile(ctx.Server.MapPath("~/"), prpFile, prpsquashWS) & ")()"

    Dim f As String
    Dim CacheKey = "VueJSWebForm:" & ctx.Request.MapPath(prpFile)
    Dim obj = ctx.Cache.Get(CacheKey)
    If obj Is Nothing Then
      Dim FileList As New List(Of String)
      f = VueFilesToJS.Compile(ctx.Server.MapPath("~/"), prpFile, prpsquashWS, Nothing, AddressOf FileList.Add)
      ctx.Cache.Add(CacheKey, f, New Web.Caching.CacheDependency(FileList.ToArray), Web.Caching.Cache.NoAbsoluteExpiration, Web.Caching.Cache.NoSlidingExpiration, Web.Caching.CacheItemPriority.Normal, Nothing)
    Else
      f = DirectCast(obj, String)
    End If

    Return "(" & f & ")()"
  End Function

End Module
