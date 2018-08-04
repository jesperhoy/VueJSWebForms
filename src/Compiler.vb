Public Class CompilerOptions
  Public Components As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
End Class

Public Class Compiler

  Private Shared ReadOnly SelfClosers As String() = {"area", "base", "br", "col", "command", "embed", "hr", "img", "input", "keygen", "link", "menuitem", "meta", "param", "source", "track", "wbr"}

  Public Shared Function Compile(template As String, Optional co As CompilerOptions = Nothing) As String
    If co Is Nothing Then co = New CompilerOptions

    Dim CurElem = New HtmlElement With {.Name = "~ROOT~"}
    Dim HtmlStack As New Stack(Of HtmlElement)
    Dim NewElem As HtmlElement
    Dim i, j As Integer
    Dim x, y As String

    template = template.Trim
    Do
      i = template.IndexOf("<"c)
      If i < 0 Then Exit Do
      j = FindEndOfHtmlTag(template, i + 1)
      If j < 0 Then Exit Do
      If i > 0 Then CurElem.SubItems.Add(New HtmlElement With {.Text = template.Substring(0, i)})
      x = template.Substring(i + 1, j - i - 1)
      template = template.Substring(j + 1)

      REM element name
      i = x.IndexOf(" "c)
      If i < 0 Then i = x.Length
      y = x.Substring(0, i).ToLower
      x = x.Substring(i)
      REM closing tag?
      If y.StartsWith("/") Then
        y = y.Substring(1)
        If y <> CurElem.Name Then Throw New Exception("Start/end tag name mismatch: " & CurElem.Name & " / " & y)
        CurElem = HtmlStack.Pop
        Continue Do
      End If
      NewElem = New HtmlElement With {.Name = y}

      REM self closing?
      If x.EndsWith("/") Then
        NewElem.SelfClosing = True
        x = x.Substring(0, x.Length - 1)
      Else
        NewElem.SelfClosing = SelfClosers.Contains(y)
      End If

      NewElem.ParseAttribs(x)

      CurElem.SubItems.Add(NewElem)
      If Not NewElem.SelfClosing Then
        HtmlStack.Push(CurElem)
        CurElem = NewElem
      End If
    Loop
    If HtmlStack.Count > 0 Then Throw New Exception("Mismatch start/end tags")
    If template.Length > 0 Then CurElem.SubItems.Add(New HtmlElement With {.Text = template})

    If CurElem.SubItems.Count = 0 Then Throw New Exception("Template is empty")
    If CurElem.SubItems.Count > 1 Then Throw New Exception("Multiple root elements")
    If CurElem.SubItems(0).Text IsNot Nothing Then Throw New Exception("Root elements is text")

    Dim XI = CurElem.SubItems(0).RenderXI(co)
    XI.Reduce()

    Dim sb = New System.Text.StringBuilder
    sb.AppendLine("function(_data) {")
    sb.AppendLine("  with(_data){")
    REM client HTML encode helper function
    sb.AppendLine("    function _he(x){return x.toString().split('&').join('&amp;').split('<').join('&lt;').split('>').join('&gt;').split('""').join('&quot;')}")
    REM client Attribute render helper function
    sb.AppendLine("    function _at(n,v){" &
                            "var tp=typeof v;" &
                            "if(tp==='string') return ' '+n+'=""'+_he(v)+'""';" &
                            "if(tp==='number') return ' '+n+'=""'+v+'""';" &
                            "if(tp==='boolean') return v?' '+n:'';" &
                            "if(tp==='object'){" &
                              "if(n==='class' && Array.isArray(v)) return ' '+n+'=""'+v.join(' ')+'""';" &
                            "};" &
                            "throw tp +' not supported';" &
                          "}")
    REM client for-loop render helper function
    sb.AppendLine("    function _fr(c,r){" &
                            "var rv='';" &
                            "if(Array.isArray(c)){" &
                              "for(var i=0;i<c.length;i++){" &
                                "rv+=r(c[i],i);" &
                              "};" &
                              "return rv;" &
                            "};" &
                            "if(typeof c==='number'){" &
                              "for(var i=0;i<c;i++){" &
                                "rv+=r(i+1,i);" &
                              "};" &
                              "return rv;" &
                            "};" &
                            "throw 'type ' + (typeof c) + ' not supported';" &
                         "}")
    sb.AppendLine("    return " & XI.RenderVal())
    sb.AppendLine("  }")
    sb.Append("}")
    Return sb.ToString
  End Function

  Private Shared Function FindEndOfHtmlTag(inStr As String, startIndex As Integer) As Integer
    Dim inDQ = False, inSQ = False
    Dim cct = 0
    Dim cc As Char
    For i = startIndex To inStr.Length - 1
      cc = inStr(i)
      Select Case cc
        Case ">"c
          If Not inDQ AndAlso Not inSQ Then Return i
        Case """"c
          If Not inSQ Then inDQ = Not inDQ
        Case "'"c
          If Not inDQ Then inSQ = Not inSQ
      End Select
    Next
    Return -1
  End Function


  Private Class HtmlElement
    Public Text As String = Nothing 'special internal type - used for text between elements
    Public Name As String
    Public SelfClosing As Boolean = False
    Public Attribs As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
    Public SubItems As New List(Of HtmlElement)

    Public Sub ParseAttribs(x As String)
      Dim i As Integer
      Dim y As String
      Do
        x = x.Trim
        If x.Length = 0 Then Exit Sub
        i = x.IndexOfAny({" "c, "="c})
        If i < 0 Then
          If Attribs.ContainsKey(x.ToLower) Then Throw New Exception("HTML element '" & Name & "' has multiple '" & x & "' attributes")
          Attribs.Add(x, Nothing)
          Exit Sub
        End If
        y = x.Substring(0, i)
        If x(i) = " "c Then
          If Attribs.ContainsKey(y.ToLower) Then Throw New Exception("HTML element '" & Name & "' has multiple '" & y & "' attributes")
          Attribs.Add(y, Nothing)
          x = x.Substring(i + 1)
          Continue Do
        End If
        x = x.Substring(i + 1).Trim
        If x.Length = 0 Then Throw New Exception("Unclosed attribute '" & y & "' on tag '" & Name & "'")
        If x(0) = """"c Or x(0) = "'" Then
          i = x.IndexOf(x(0), 1)
          If i < 0 Then Throw New Exception("Unclosed attribute '" & y & "' on tag '" & Name & "'")
        Else
          i = x.IndexOf(" "c)
        End If
        If Attribs.ContainsKey(y.ToLower) Then Throw New Exception("HTML element '" & Name & "' has multiple '" & y & "' attributes")
        Attribs.Add(y, x.Substring(1, i - 1))
        x = x.Substring(i + 1)
      Loop
    End Sub

    Friend Function RenderXI(co As CompilerOptions) As IExecItem
      If Text IsNot Nothing Then Return RenderXIText(co)

      REM check for v-pre 
      If Attribs.ContainsKey("v-pre") Then
        Attribs.Remove("v-pre")
        Return RenderXIPre(co)
      End If

      REM wrap all subitem v-for elements in a <template v-for="..."> tag - moving "v-for", ":key", "v-bind:key" up to the template
      Dim AttrVal As String = Nothing
      For i = 0 To SubItems.Count - 1
        With SubItems(i)
          If .Attribs.ContainsKey("v-pre") Then Continue For
          If .Attribs.TryGetValue("v-for", AttrVal) Then
            Dim he = New HtmlElement
            he.Name = "template"
            .Attribs.Remove("v-for")
            he.Attribs.Add("v-for", AttrVal)
            If .Attribs.TryGetValue(":key", AttrVal) Then
              .Attribs.Remove(":key")
              he.Attribs.Add(":key", AttrVal)
            End If
            If .Attribs.TryGetValue("v-bind:key", AttrVal) Then
              .Attribs.Remove("v-bind:key")
              he.Attribs.Add("v-bind:key", AttrVal)
            End If
            he.SubItems.Add(SubItems(i))
            SubItems(i) = he
          End If
        End With
      Next

      REM for loop
      If Attribs.TryGetValue("v-for", AttrVal) Then Return RenderXIForLoop(AttrVal, co)

      REM template? don't render tag itself - only content
      If Name = "template" Then Return RenderSubItemsXI(co)

      REM component?
      Dim fn As String = Nothing
      If co.Components.TryGetValue(Name, fn) Then Return RenderXIComponent(fn, co)

      Return RenderXIElement(co)
    End Function

    Private Function RenderXIComponent(fn As String, co As CompilerOptions) As IExecItem
      If SubItems.Count > 0 Then Throw New Exception("Component content is not (yet) supported: " & Name)

      Dim x = "$parent:_data," &
              "$root:(""$root"" in _data?_data.$root:_data)"

      Dim AttrName As String = Nothing
      Dim AttrVal As String = Nothing
      For Each kv In Attribs
        AttrName = kv.Key.ToLower
        AttrVal = kv.Value
        If AttrName.StartsWith("v-bind:") OrElse AttrName.StartsWith(":") Then
          Dim an = kv.Key.Substring(If(kv.Key.StartsWith(":"), 1, 7))
          If AttrVal.StartsWith("{") Then Throw New Exception("Binding with Objects not supported")
          x &= ",""" & an.Replace("\", "\\").Replace("""", "\""") & """:" & AttrVal
        ElseIf AttrName = "v-html" OrElse AttrName = "v-text" OrElse AttrName = "v-show" Then
          Throw New Exception(AttrName & "not supported on components")
        ElseIf AttrName = "v-once" Or kv.Key = "v-cloak" Then
          REM do nothing - just remove it
        ElseIf kv.Key.StartsWith("v-") OrElse kv.Key.StartsWith("@") Then
          Throw New NotImplementedException("Vue attribute '" & AttrName & "' not implemented")
        Else
          If AttrVal Is Nothing Then
            x &= ",""" & kv.Key.Replace("\", "\\").Replace("""", "\""") & """:true"
          Else
            x &= ",""" & kv.Key.Replace("\", "\\").Replace("""", "\""") & """:""" & AttrVal.Replace("\", "\\").Replace("""", "\""") & """"
          End If
        End If
      Next
      Return New ExecItemEval With {.JS = fn & "({" & x & "})"}
    End Function

    Private Function RenderXIElement(co As CompilerOptions) As ExecItemList
      Dim AttrVal As String = Nothing

      Dim rv = New ExecItemList
      rv.Items.Add(New ExecItemRaw With {.Text = "<" & Name})

      If Attribs.TryGetValue("v-show", AttrVal) Then
        Attribs.Remove("v-show")
        If Attribs.ContainsKey(":style") Or Attribs.ContainsKey("v-bind:style") Then Throw New Exception("v-show is not supported together with bound style attribute")
        Dim attr2Val As String = Nothing
        Dim y = ""
        If Attribs.TryGetValue("style", attr2Val) Then
          Attribs.Remove("style")
          y = attr2Val.TrimEnd(";".ToArray)
        End If
        If y.Length > 0 Then
          rv.Items.Add(New ExecItemEval With {.JS = "(' style=""'" & y & "'+(" & AttrVal & "?'':';display:none'))+'""')"})
        Else
          rv.Items.Add(New ExecItemEval With {.JS = "(" & AttrVal & "?'':' style=""display:none""')"})
        End If
      End If

      Dim AttrName As String = Nothing
      Dim VHtml As String = Nothing, VText As String = Nothing
      For Each kv In Attribs
        AttrName = kv.Key.ToLower
        AttrVal = kv.Value
        If AttrName.StartsWith("v-bind:") OrElse AttrName.StartsWith(":") Then
          Dim an = kv.Key.Substring(If(AttrName.StartsWith(":"), 1, 7))
          If AttrVal.StartsWith("{") Then Throw New Exception("Binding with Objects not supported")
          rv.Items.Add(New ExecItemEval With {.JS = "_at('" & an & "'," & AttrVal & ")"})
        ElseIf AttrName = "v-html" Then
          VHtml = AttrVal
        ElseIf AttrName = "v-text" Then
          VText = AttrVal
        ElseIf AttrName = "v-once" Or kv.Key = "v-cloak" Then
          REM do nothing - just remove it
        ElseIf kv.Key.StartsWith("v-") OrElse kv.Key.StartsWith("@") Then
          Throw New NotImplementedException("Vue attribute '" & AttrName & "' not implemented")
        Else
          If AttrVal Is Nothing Then
            rv.Items.Add(New ExecItemRaw With {.Text = " " & kv.Key})
          Else
            rv.Items.Add(New ExecItemRaw With {.Text = " " & kv.Key & "=""" & AttrVal & """"})
          End If
        End If
      Next

      If SelfClosing And SubItems.Count = 0 Then
        rv.Items.Add(New ExecItemRaw With {.Text = "/>"})
        Return rv
      End If

      rv.Items.Add(New ExecItemRaw With {.Text = ">"})

      If VHtml IsNot Nothing Then
        rv.Items.Add(New ExecItemEval With {.JS = "(" & VHtml & ")"})
      ElseIf VText IsNot Nothing Then
        rv.Items.Add(New ExecItemEval With {.JS = "_he(" & VText & ")"})
      Else
        rv.Items.Add(RenderSubItemsXI(co))
      End If

      rv.Items.Add(New ExecItemRaw With {.Text = "</" & Name & ">"})
      Return rv
    End Function

    Private Function RenderSubItemsXI(co As CompilerOptions) As ExecItemList
      Dim AttrVal As String = Nothing
      Dim rv = New ExecItemList
      Dim i = 0
      While i < SubItems.Count
        With SubItems(i)
          If Not .Attribs.ContainsKey("v-pre") Then
            If SubItems(i).Attribs.TryGetValue("v-if", AttrVal) Then
              SubItems(i).Attribs.Remove("v-if")
              rv.Items.Add(RenderSubItemConditional(AttrVal, i, co)) 'i is passed byref and will be incremented by function
              Continue While
            End If
            If SubItems(i).Attribs.ContainsKey("v-else-if") Then Throw New Exception("Unexpected v-else-if statement - no preceeding v-if")
            If SubItems(i).Attribs.ContainsKey("v-else") Then Throw New Exception("Unexpected v-else statement - no preceeding v-if")
          End If
          rv.Items.Add(SubItems(i).RenderXI(co))
        End With
        i += 1
      End While
      Return rv
    End Function

    Private Function RenderSubItemConditional(condJS As String, ByRef pos As Integer, co As CompilerOptions) As ExecItemConditional
      Dim rv = New ExecItemConditional
      rv.JS = condJS
      rv.ExecTrue = SubItems(pos).RenderXI(co)
      REM special case if element contains both v-if and v-for 
      If SubItems(pos).Attribs.ContainsKey("v-for") Then pos += 1 : Return rv
      pos += 1

      If pos >= SubItems.Count Then Return rv

      REM if next item is white space, and the item after that is v-else-if/v-else then skip the white space
      If pos < SubItems.Count - 1 AndAlso
             SubItems(pos).Text IsNot Nothing AndAlso SubItems(pos).Text.Trim.Length = 0 AndAlso
             SubItems(pos + 1).Text Is Nothing AndAlso
             (SubItems(pos + 1).Attribs.ContainsKey("v-else-if") OrElse SubItems(pos + 1).Attribs.ContainsKey("v-else")) Then
        pos += 1
      End If

      If SubItems(pos).Text IsNot Nothing Then Return rv

      Dim AttrVal As String = Nothing
      If SubItems(pos).Attribs.TryGetValue("v-else-if", AttrVal) Then
        SubItems(pos).Attribs.Remove("v-else-if")
        rv.ExecFalse = RenderSubItemConditional(AttrVal, pos, co)
      ElseIf SubItems(pos).Attribs.ContainsKey("v-else") Then
        SubItems(pos).Attribs.Remove("v-else")
        rv.ExecFalse = SubItems(pos).RenderXI(co)
        pos += 1
      End If
      Return rv
    End Function

    Private Function RenderXIForLoop(forExpr As String, co As CompilerOptions) As ExecItemFor
      Dim x = forExpr
      Dim rv = New ExecItemFor
      Dim i As Integer
      If x.StartsWith("(") Then
        x = x.Substring(1).Trim
        i = x.IndexOf(",")
        If i <= 0 Then Throw New Exception("Invalid for expression: " & forExpr)
        rv.ItemVar = x.Substring(0, i).Trim
        x = x.Substring(i + 1).Trim
        i = x.IndexOf(")")
        If i <= 0 Then Throw New Exception("Invalid for expression: " & forExpr)
        rv.IdxVar = x.Substring(0, i).Trim
        x = x.Substring(i + 1).Trim
      Else
        i = x.IndexOf(" ")
        If i <= 0 Then Throw New Exception("Invalid for expression: " & forExpr)
        rv.ItemVar = x.Substring(0, i)
        x = x.Substring(i + 1).Trim
      End If
      If Not x.StartsWith("in ") Then Throw New Exception("Invalid for expression: " & forExpr)
      rv.ArrayVar = x.Substring(3).Trim
      rv.LoopOn = RenderSubItemsXI(co)
      Return rv
    End Function

    Private Function RenderXIText(co As CompilerOptions) As ExecItemList
      Dim rv = New ExecItemList
      Dim x = Text
      Dim i, j As Integer
      Do
        i = x.IndexOf("{{")
        If i < 0 Then Exit Do
        j = x.IndexOf("}}", i + 2)
        If j < 0 Then Exit Do
        If i > 0 Then rv.Items.Add(New ExecItemRaw With {.Text = x.Substring(0, i)})
        rv.Items.Add(New ExecItemEval With {.JS = "_he(" & x.Substring(i + 2, j - i - 2).Trim & ")"})
        x = x.Substring(j + 2)
      Loop
      If x.Length > 0 Then rv.Items.Add(New ExecItemRaw With {.Text = x})
      Return rv
    End Function

    Private Function RenderXIPre(co As CompilerOptions) As IExecItem
      If Text IsNot Nothing Then Return New ExecItemRaw With {.Text = Text}
      Dim rv = New ExecItemList
      rv.Items.Add(New ExecItemRaw With {.Text = "<" & Name})
      For Each kv In Attribs
        rv.Items.Add(New ExecItemRaw With {.Text = " " & kv.Key})
        If kv.Value IsNot Nothing Then rv.Items.Add(New ExecItemRaw With {.Text = "=""" & kv.Value & """"})
      Next
      If SelfClosing And SubItems.Count = 0 Then
        rv.Items.Add(New ExecItemRaw With {.Text = "/>"})
        Return rv
      End If
      rv.Items.Add(New ExecItemRaw With {.Text = ">"})
      For Each itm In SubItems
        rv.Items.Add(itm.RenderXIPre(co))
      Next
      rv.Items.Add(New ExecItemRaw With {.Text = "</" & Name & ">"})
      Return rv
    End Function

  End Class



  Private Interface IExecItem
    Function RenderVal() As String
    Sub Reduce()
  End Interface

  Private Class ExecItemList
    Implements IExecItem

    Public Items As New List(Of IExecItem)

    Public Sub Reduce() Implements IExecItem.Reduce
      For Each itm In Items
        itm.Reduce()
      Next

      REM pull up sub-lists
      Dim i = 0
      Do While i < Items.Count
        If TypeOf Items(i) Is ExecItemList Then
          Dim sl = DirectCast(Items(i), ExecItemList)
          Items.RemoveAt(i)
          For Each itm In sl.Items
            Items.Insert(i, itm)
            i += 1
          Next
          Continue Do
        End If
        i += 1
      Loop

      REM merge raw items 
      i = 0
      Do While i < Items.Count - 1
        If TypeOf Items(i) Is ExecItemRaw AndAlso TypeOf Items(i + 1) Is ExecItemRaw Then
          Items(i) = New ExecItemRaw With {.Text = DirectCast(Items(i), ExecItemRaw).Text & DirectCast(Items(i + 1), ExecItemRaw).Text}
          Items.RemoveAt(i + 1)
          Continue Do
        End If
        i += 1
      Loop
    End Sub

    Public Function RenderVal() As String Implements IExecItem.RenderVal
      If Items.Count = 0 Then Return "''"
      Dim sb = New System.Text.StringBuilder
      For Each itm In Items
        If sb.Length > 0 Then sb.Append("+")
        sb.Append(itm.RenderVal())
      Next
      Return sb.ToString
    End Function

  End Class

  Private Class ExecItemRaw
    Implements IExecItem

    Public Text As String

    Public Sub Reduce() Implements IExecItem.Reduce
      REM no sub-lists
    End Sub

    Public Function RenderVal() As String Implements IExecItem.RenderVal
      Return "'" & JSEncodeHtml(Text) & "'"
    End Function

    Private Function JSEncodeHtml(x As String) As String
      x = x.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbTab, " ")
      Dim sb = New System.Text.StringBuilder
      Dim p = 0
      Dim i As Integer
      While p < x.Length
        i = x.IndexOf("  ", p)
        If i < 0 Then sb.Append(x.Substring(p)) : Exit While
        sb.Append(x.Substring(p, i - p + 1))
        p = i + 2
        While p < x.Length AndAlso x(p) = " "c
          p += 1
        End While
      End While
      Return sb.ToString.Replace("\", "\\").Replace("'", "\'")
    End Function
  End Class

  Private Class ExecItemEval
    Implements IExecItem

    Public JS As String

    Public Sub Reduce() Implements IExecItem.Reduce
      REM no sub-lists
    End Sub

    Public Function RenderVal() As String Implements IExecItem.RenderVal
      Return JS
    End Function

  End Class

  Private Class ExecItemConditional
    Implements IExecItem

    Public JS As String
    Public ExecTrue As IExecItem
    Public ExecFalse As IExecItem

    Public Sub Reduce() Implements IExecItem.Reduce
      ExecTrue.Reduce()
      If ExecFalse IsNot Nothing Then ExecFalse.Reduce()
    End Sub

    Public Function RenderVal() As String Implements IExecItem.RenderVal
      Dim sb = New System.Text.StringBuilder
      sb.Append("(" & JS & "?(")
      Dim x = ExecTrue.RenderVal()
      sb.Append(If(x.Length > 0, x, "''"))
      sb.Append("):(")
      x = If(ExecFalse Is Nothing, "", ExecFalse.RenderVal())
      sb.Append(If(x.Length > 0, x, "''"))
      sb.Append("))")
      Return sb.ToString
    End Function

  End Class

  Private Class ExecItemFor
    Implements IExecItem

    Public ItemVar As String
    Public ArrayVar As String
    Public IdxVar As String
    Public LoopOn As IExecItem

    Public Sub Reduce() Implements IExecItem.Reduce
      LoopOn.Reduce()
    End Sub

    Public Function RenderVal() As String Implements IExecItem.RenderVal
      If IdxVar Is Nothing Then IdxVar = ItemVar & "_ix"
      Return "_fr(" & ArrayVar & ",function(" & ItemVar & "," & IdxVar & "){return " & LoopOn.RenderVal() & "})"
    End Function

  End Class

End Class

