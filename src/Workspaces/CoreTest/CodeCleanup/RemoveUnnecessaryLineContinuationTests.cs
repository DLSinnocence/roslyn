﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeCleanup;
using Microsoft.CodeAnalysis.CodeCleanup.Providers;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests.CodeCleanup;

[UseExportProvider]
[Trait(Traits.Feature, Traits.Features.RemoveUnnecessaryLineContinuation)]
public sealed class RemoveUnnecessaryLineContinuationTests
{
    [Fact]
    public async Task ColonTrivia()
    {
        var code = @"[|
        ::: Console.WriteLine("")|]";

        var expected = @"
        Console.WriteLine("")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonTrivia_EndOfLine()
    {
        var code = @"[|
        ::: 

        Console.WriteLine("")|]";

        var expected = @"


        Console.WriteLine("")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonTrivia_LineContinuation()
    {
        var code = @"[|
        ::: _
        _
        _
        Console.WriteLine("")|]";

        var expected = @"



        Console.WriteLine("")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonTrivia_LineContinuation2()
    {
        var code = @"[|
        ::: 
        _
        _
        Console.WriteLine("")|]";

        var expected = @"



        Console.WriteLine("")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonTrivia_LineContinuation3()
    {
        var code = @"[|
        ::: 
        _
        
        Console.WriteLine("")|]";

        var expected = @"



        Console.WriteLine("")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonTrivia_LineContinuation_Comment()
    {
        var code = @"[|
        ::: 
        _
        ' test
        Console.WriteLine("")|]";

        var expected = @"

                       _
        ' test
        Console.WriteLine("")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task LineContinuation()
    {
        var code = @"[|
        Console.WriteLine("""") _

        Console.WriteLine("""")|]";

        var expected = @"
        Console.WriteLine("""")

        Console.WriteLine("""")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task LineContinuation_MultipleLines()
    {
        var code = @"[|
        Console.WriteLine("""") _
        _
        _
        Console.WriteLine("""")|]";

        var expected = @"
        Console.WriteLine("""") _
        _
        _
        Console.WriteLine("""")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task LineContinuation_MultipleLines2()
    {
        var code = @"[|
        Console.WriteLine("""") _
        _
        _

        Console.WriteLine("""")|]";

        var expected = @"
        Console.WriteLine("""")



        Console.WriteLine("""")";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task LineContinuation_Invalid()
    {
        var code = @"[|
         Console.WriteLine() _             _ 
        ' test 
        : ' test
        _
        Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _             _ 
        ' test 
         ' test
        _
        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_SingleLine()
    {
        var code = @"[|
         Console.WriteLine() : Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() : Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_SingleLine_MultipleColon()
    {
        var code = @"[|
         Console.WriteLine() :::: Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() : Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_SingleLine_SkippedTokens()
    {
        var code = @"[|
         Console.WriteLine() _ : Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _ : Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_BeforeColonToken()
    {
        var code = @"[|
         Console.WriteLine() _ 
         : Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine()
        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_BeforeColonToken2()
    {
        var code = @"[|
         Console.WriteLine() _  _
         : Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _  _
          Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_Comment_BeforeColonToken()
    {
        var code = @"[|
         Console.WriteLine() _ ' test
         : Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _ ' test
        Console.WriteLine()";
        await VerifyAsync(CreateMethod(code), CreateMethod(expected), LanguageVersion.VisualBasic15);
    }

    [Fact]
    public async Task ColonToken_LineContinuation_Comment_BeforeColonTokenV16()
    {
        var code = @"[|
         Console.WriteLine() _ ' test
         : Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _ ' test
        Console.WriteLine()";
        await VerifyAsync(CreateMethod(code), CreateMethod(expected), LanguageVersion.VisualBasic16);
    }

    [Fact]
    public async Task ColonToken_MultipleLine()
    {
        var code = @"[|
         Console.WriteLine() : 
         Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine()
        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_AfterColonToken()
    {
        var code = @"[|
         Console.WriteLine() : _
         Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine()
        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_AfterColonToken2()
    {
        var code = @"[|
         Console.WriteLine() : _
         _
         Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine()

        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_AfterColonToken_MultipleLine()
    {
        var code = @"[|
         Console.WriteLine() : _
         _
         _|]";

        var expected = @"
        Console.WriteLine()

";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_AfterColonToken_Mixed()
    {
        var code = @"[|
         Console.WriteLine() : _
         _
         :
         _
         Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine()



        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_AfterColonToken_Colon_Comment()
    {
        var code = @"[|
         Console.WriteLine() : _
         _
         : ' test
         _
         Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _
                            _
        ' test
        _
        Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_LineContinuation_Mix()
    {
        var code = @"[|
         Console.WriteLine() _ : _
         _
         : ' test
         _
         Console.WriteLine()|]";

        var expected = @"
        Console.WriteLine() _  _
         _
          ' test
         _
         Console.WriteLine()";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ColonToken_If()
    {
        var code = @"[|
        If True Then :
        End If|]";

        var expected = @"
        If True Then
        End If";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ImplicitLineContinuation()
    {
        var code = @"[|
        Dim i = _
                1 + _
                2|]";

        var expected = @"
        Dim i =
                1 +
                2";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task ImplicitLineContinuation_Multiple()
    {
        var code = @"[|
        Dim i = _
                _
                1 + _
                2|]";

        var expected = @"
        Dim i = _
                _
                1 +
                2";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact]
    public async Task LineContinuation_Mix()
    {
        var code = @"[|Class _
 A
    Inherits _
        System _
        . _
        Object

    Public _
        Function _
            Method _
                ( _
                    i _
                        As _
                            Integer _
                            , _
                    i2 _
                        As _
                            String _
                ) _
                As _
                Integer

        If _
            i _
                + _
                    i2 _
                    . _
                    Length _
                        = _
                            1 _
                                Then
            Console _
                . _
                    WriteLine _
                    ( _
                        vbCrLf _
                    )
        End _
            If

        Return _
            1
    End _
        Function
End _
    Class|]";

        var expected = @"Class _
 A
    Inherits _
        System _
        .
        Object

    Public _
        Function _
            Method _
                (
                    i _
                        As _
                            Integer _
                            ,
                    i2 _
                        As _
                            String
                ) _
                As _
                Integer

        If _
            i _
                +
                    i2 _
                    .
                    Length _
                        =
                            1 _
                                Then
            Console _
                .
                    WriteLine _
                    (
                        vbCrLf
                    )
        End _
            If

        Return _
            1
    End _
        Function
End _
    Class";

        await VerifyAsync(code, expected);
    }

    [Fact]
    public async Task ImplicitLineContinuation_Invalid()
    {
        var code = @"[|
        Dim i = _ _
                _ _
                1 + _ _
                2|]";

        var expected = @"
        Dim i = _ _
                _ _
                1 + _ _
                2";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544470")]
    public async Task AttributeTargetColon()
    {
        var code = @"[|<Assembly: _
CLSCompliant>|]";

        var expected = @"<Assembly: _
CLSCompliant>";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529428")]
    public async Task LineContinuationInImport()
    {
        var code = @"[|Imports System _

|]";

        var expected = @"Imports System

";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529425")]
    public async Task ColonInOption()
    {
        var code = @"[|Option Infer On :: Option Explicit Off|]";

        var expected = @"Option Infer On : Option Explicit Off";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544524")]
    public async Task LineContinuationInNamedFieldInitializer()
    {
        var code = @"[|Class C
    Sub S()
        Dim o = New With
            {
                . _
                a = 2
            }
    End Sub
End Class|]";

        var expected = @"Class C
    Sub S()
        Dim o = New With
            {
                . _
                a = 2
            }
    End Sub
End Class";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544523")]
    public async Task IfPart_Colon1()
    {
        var code = @"[|Module M
    Sub S()
        If True Then
            : Return : End If
    End Sub
End Module|]";

        var expected = @"Module M
    Sub S()
        If True Then
            Return : End If
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544523")]
    public async Task IfPart_Colon2()
    {
        var code = @"[|Module M
    Sub S()
        If True Then : 
            Return : End If
    End Sub
End Module|]";

        var expected = @"Module M
    Sub S()
        If True Then
            Return : End If
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544523")]
    public async Task IfPart_Colon3()
    {
        var code = @"[|Module M
    Sub S()
        If True Then : Return
        : End If
    End Sub
End Module|]";

        var expected = @"Module M
    Sub S()
        If True Then : Return
        End If
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544523")]
    public async Task IfPart_Colon4()
    {
        var code = @"[|Module M
    Sub S()
        If True Then : Return : 
        End If
    End Sub
End Module|]";

        var expected = @"Module M
    Sub S()
        If True Then : Return
        End If
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544521")]
    public async Task LabelColon()
    {
        var code = @"[|Module Program
    Sub S()
        L: 
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub S()
L:
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544521")]
    public async Task LabelColon_ColonTrivia()
    {
        var code = @"[|Module Program
    Sub S()
        L:::::::::  
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub S()
L:
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544520")]
    public async Task LineContinuation_MixedWithImplicitLineContinuation()
    {
        var code = @"[|Module Program
    Sub Main(
 _
        args _
        As String)
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main(
             _
        args _
        As String)
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544549")]
    public async Task ColonTrivia_EndOfFile()
    {
        var code = @"[|:::::::
|]";

        var expected = @"
";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545538")]
    public async Task ColonTriviaBeforeCommentTrivia()
    {
        var code = @"[|Module M
    Sub Main()
        Dim b = <x/>.@x : '
    End Sub
End Module|]";

        var expected = @"Module M
    Sub Main()
        Dim b = <x/>.@x  '
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545540")]
    public async Task InsideWithStatementWithMemberCall()
    {
        var code = @"[|Module Program
    Sub Main()
        With ""
            Dim y = From x In "" Distinct
            : .ToLower()
        End With
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main()
        With ""
            Dim y = From x In "" Distinct
            : .ToLower()
        End With
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545540")]
    public async Task InsideWithStatementWithMemberCall2()
    {
        var code = @"[|Module Program
    Sub Main()
        With ""
            Dim y = From x In """" Distinct :
            .ToLower()
        End With
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main()
        With ""
            Dim y = From x In """" Distinct :
            .ToLower()
        End With
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545540")]
    public async Task InsideWithStatementWithMemberCall3()
    {
        var code = @"[|Module Program
    Sub Main()
        With ""
            .ToLower()
            : .ToLower()
        End With
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main()
        With ""
            .ToLower()
            : .ToLower()
        End With
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545540")]
    public async Task InsideWithStatementWithMemberCall4()
    {
        var code = @"[|Module Program
    Sub Main()
        With """"
            .ToLower() :
            .ToLower()
        End With
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main()
        With """"
            .ToLower()
            .ToLower()
        End With
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/607791")]
    public async Task InsideWithStatementWithDictionaryAccess()
    {
        var code = @"[|Imports System.Collections
Module Program
    Sub Main()
        With New Hashtable
            Dim x = From c In """" Distinct
            :!A = !B
        End With
    End Sub
End Module
|]";

        var expected = @"Imports System.Collections
Module Program
    Sub Main()
        With New Hashtable
            Dim x = From c In """" Distinct
            : !A = !B
        End With
    End Sub
End Module
";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/607791")]
    public async Task InsideWithStatementWithDictionaryAccess2()
    {
        var code = @"[|Imports System.Collections
Module Program
    Sub Main()
        With New Hashtable
            Dim x = From c In """" Distinct :
              !A = !B
        End With
    End Sub
End Module|]";

        var expected = @"Imports System.Collections
Module Program
    Sub Main()
        With New Hashtable
            Dim x = From c In """" Distinct :
            !A = !B
        End With
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/529821")]
    public async Task InsideObjectInitializer()
    {
        var code = @"[|Imports System.Runtime.CompilerServices
 
Module Program
    Sub Main()
        Dim s = New StrongBox(Of Object) With {
        .Value = Sub()
                     Dim y = From x In "" Distinct
                     : .Value.ToString()
                 End Sub}
    End Sub
End Module|]";

        var expected = @"Imports System.Runtime.CompilerServices

Module Program
    Sub Main()
        Dim s = New StrongBox(Of Object) With {
        .Value = Sub()
                     Dim y = From x In "" Distinct
                     : .Value.ToString()
                 End Sub}
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545545")]
    public async Task LineContinuationBetweenXmlAndDot()
    {
        var code = @"[|Module Program
    Sub Main()
        Dim y = <?xml version=""1.0""?><root/> _
        .ToString()
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main()
        Dim y = <?xml version=""1.0""?><root/> _
        .ToString()
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545545")]
    public async Task LineContinuationBetweenXmlAndDot1()
    {
        var code = @"[|Module Program
    Sub Main()
        Dim x = <x/>.. _
            .<x>
    End Sub
End Module|]";

        var expected = @"Module Program
    Sub Main()
        Dim x = <x/>.. _
            .<x>
    End Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545565")]
    public async Task LineContinuationBeforeFromQueryExpression()
    {
        var code = @"[|Class C
    Sub Main()
        Call _
        From x In """" Distinct.ToString()
    End Sub
End Class|]";

        var expected = @"Class C
    Sub Main()
        Call _
        From x In """" Distinct.ToString()
    End Sub
End Class";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545565")]
    public async Task LineContinuationBeforeFromAggregateExpression()
    {
        var code = @"[|Class C
    Sub Main()
        Call _
            Aggregate x In {1} Into Count().ToString()
    End Sub
End Class|]";

        var expected = @"Class C
    Sub Main()
        Call _
            Aggregate x In {1} Into Count().ToString()
    End Sub
End Class";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530635")]
    public async Task LineContinuationAtEndOfLambdaExpression1()
    {
        var code = @"[|Interface I
    Property A As Action
End Interface
 
Class C
    Implements I
    Property A As Action = Sub() Return _
    Implements I.A
End Class|]";

        var expected = @"Interface I
    Property A As Action
End Interface

Class C
    Implements I
    Property A As Action = Sub() Return _
    Implements I.A
End Class";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530635")]
    public async Task LineContinuationAtEndOfLambdaExpression2()
    {
        var code = @"[|Interface I
    Property A As Action
End Interface
 
Class C
    Implements I
    Property A As Action = Sub()
                               Return
                           End Sub _
    Implements I.A
End Class|]";

        var expected = @"Interface I
    Property A As Action
End Interface

Class C
    Implements I
    Property A As Action = Sub()
                               Return
                           End Sub _
    Implements I.A
End Class";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546798")]
    public async Task LineContinuationAfterDot()
    {
        var code = @"[|
        System.Diagnostics. _
            Debug.Assert(True)|]";

        var expected = @"
        System.Diagnostics.
            Debug.Assert(True)";

        await VerifyAsync(CreateMethod(code), CreateMethod(expected));
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530621")]
    [WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/631933")]
    public async Task DoNotRemoveLineContinuationAfterColonInSingleLineIfStatement()
    {
        var code = @"[|Module Program
    Dim x = Sub() If True Then Dim y : _
                               Exit Sub
End Module|]";

        var expected = @"Module Program
    Dim x = Sub() If True Then Dim y : _
                               Exit Sub
End Module";

        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/609481")]
    [WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/631933")]
    public async Task DoNotRemoveLineContinuationInSingleLineIfStatement()
    {
        var code = @"[|
Module Program
    Sub Main()
        ' Single Line If with explicit line continuations
        If True Then _
            Return _
        Else _
        Return
    End Sub
End Module
|]";

        var expected = @"
Module Program
    Sub Main()
        ' Single Line If with explicit line continuations
        If True Then _
            Return _
        Else _
        Return
    End Sub
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/609481")]
    [WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/631933")]
    public async Task DoNotRemoveLineContinuationInNestedSingleLineIfStatement()
    {
        var code = @"[|
Module Program
    Sub Main()
        ' Nested Single Line If with explicit line continuations
        If True Then _
            If True Then _
            Return _
            Else _
            Return _
        Else _
        Return

        If True Then _
            If True Then _
            Return _
            Else _
            Return _
        Else _
            If True Then _
            Return _
            Else Return

    End Sub
End Module
|]";

        var expected = @"
Module Program
    Sub Main()
        ' Nested Single Line If with explicit line continuations
        If True Then _
            If True Then _
            Return _
            Else _
            Return _
        Else _
        Return

        If True Then _
            If True Then _
            Return _
            Else _
            Return _
        Else _
            If True Then _
            Return _
            Else Return

    End Sub
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/710")]
    public async Task DoNotRemoveLineContinuationInStringInterpolation1()
    {
        var code = @"[|
Module Program
    Dim x = $""{ _
            1}""
End Module
|]";

        var expected = @"
Module Program
    Dim x = $""{ _
            1}""
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/710")]
    public async Task DoNotRemoveLineContinuationInStringInterpolation2()
    {
        var code = @"[|
Module Program
    Dim x = $""{1 _
               }""
End Module
|]";

        var expected = @"
Module Program
    Dim x = $""{1 _
               }""
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/710")]
    public async Task DoNotRemoveLineContinuationInStringInterpolation3()
    {
        var code = @"[|
Module Program
    Dim x = $""{ _

1 _

}""
End Module
|]";

        var expected = @"
Module Program
    Dim x = $""{ _

1 _

}""
End Module
";
        await VerifyAsync(code, expected);
    }

    [Theory]
    [InlineData("_")]
    [InlineData("_ ' Comment")]
    [WorkItem("https://github.com/dotnet/roslyn/issues/69696")]
    public async Task LineContinuationInString1(string continuation)
    {
        var code = $@"[|
Module Program
    Dim x = ""1"" {continuation}
            & ""2"" {continuation}
            & ""3""
End Module
|]";

        var expected = $@"
Module Program
    Dim x = ""1"" {continuation}
            & ""2"" {continuation}
            & ""3""
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/69696")]
    public async Task LineContinuationInString2()
    {
        var code = $@"[|
Module Program
    Dim x = ""1"" & _
            ""2"" & _
            ""3""
End Module
|]";

        var expected = $@"
Module Program
    Dim x = ""1"" &
            ""2"" &
            ""3""
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/69696")]
    public async Task LineContinuationInString3()
    {
        var code = $@"[|
Module Program
    Dim x = ""1"" & ' Comment
            ""2"" & ' Comment
            ""3""
End Module
|]";

        var expected = $@"
Module Program
    Dim x = ""1"" & ' Comment
            ""2"" & ' Comment
            ""3""
End Module
";
        await VerifyAsync(code, expected);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1085887")]
    public async Task DoNotRemoveLineContinuationInVisualBasic9()
    {
        var code = @"[|
Module Program
    Function Add( _
        i As Integer, _
        j As Integer, _
    ) As Integer

        Return i + j
    End Function
End Module
|]";

        var expected = @"
Module Program
    Function Add( _
        i As Integer, _
        j As Integer, _
    ) As Integer

        Return i + j
    End Function
End Module
";
        await VerifyAsync(code, expected, langVersion: LanguageVersion.VisualBasic9);
    }

    [Fact, WorkItem("http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1085887")]
    public async Task RemoveLineContinuationInVisualBasic10_11_12_And_14()
    {
        var code = @"[|
Module Program
    Function Add( _
        i As Integer, _
        j As Integer, _
    ) As Integer

        Return i + j
    End Function
End Module
|]";

        var expected = @"
Module Program
    Function Add(
        i As Integer,
        j As Integer,
    ) As Integer

        Return i + j
    End Function
End Module
";

        await VerifyAsync(code, expected, langVersion: LanguageVersion.VisualBasic10);
        await VerifyAsync(code, expected, langVersion: LanguageVersion.VisualBasic11);
        await VerifyAsync(code, expected, langVersion: LanguageVersion.VisualBasic12);
        await VerifyAsync(code, expected);
    }

    private static string CreateMethod(string body)
    {
        return @"Imports System
Class C
    Public Sub Method()" + body + @"
    End Sub
End Class";
    }

    private static async Task VerifyAsync(string codeWithMarker, string expectedResult, LanguageVersion langVersion = LanguageVersion.VisualBasic14)
    {
        MarkupTestFile.GetSpans(codeWithMarker, out var codeWithoutMarker, out var textSpans);

        var document = CreateDocument(codeWithoutMarker, LanguageNames.VisualBasic, langVersion);
        var codeCleanups = CodeCleaner.GetDefaultProviders(document).WhereAsArray(p => p.Name is PredefinedCodeCleanupProviderNames.RemoveUnnecessaryLineContinuation or PredefinedCodeCleanupProviderNames.Format);

        var cleanDocument = await CodeCleaner.CleanupAsync(document, textSpans[0], await document.GetCodeCleanupOptionsAsync(CancellationToken.None), codeCleanups);

        var actualResult = (await cleanDocument.GetRequiredSyntaxRootAsync(CancellationToken.None)).ToFullString();
        AssertEx.EqualOrDiff(expectedResult, actualResult);
    }

    private static Document CreateDocument(string code, string language, LanguageVersion langVersion)
    {
        var solution = new AdhocWorkspace().CurrentSolution;
        var projectId = ProjectId.CreateNewId();
        var project = solution
            .AddProject(projectId, "Project", "Project.dll", language)
            .GetRequiredProject(projectId);

        AssertEx.NotNull(project.ParseOptions);
        var parseOptions = (VisualBasicParseOptions)project.ParseOptions;
        parseOptions = parseOptions.WithLanguageVersion(langVersion);
        project = project.WithParseOptions(parseOptions);

        return project.AddDocument("Document", SourceText.From(code));
    }
}
