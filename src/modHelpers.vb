Module modHelpers

  Friend Function GetContextComponentList() As HashSet(Of String)
    Dim ctx = System.Web.HttpContext.Current
    Dim arr As HashSet(Of String)
    If ctx.Items.Contains("VueLightComponents") Then
      arr = DirectCast(ctx.Items("VueLightComponents"), HashSet(Of String))
    Else
      arr = New HashSet(Of String)
      ctx.Items("VueLightComponents") = arr
    End If
    Return arr
  End Function

End Module
