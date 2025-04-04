﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.ExternalAccess.UnitTesting.Api;

internal readonly partial struct UnitTestingInvocationReasons
{
    public static readonly UnitTestingInvocationReasons DocumentAdded =
        new(
            [UnitTestingPredefinedInvocationReasons.SemanticChanged]);

    public static readonly UnitTestingInvocationReasons DocumentRemoved =
        new(
            [UnitTestingPredefinedInvocationReasons.SemanticChanged, UnitTestingPredefinedInvocationReasons.HighPriority]);

    public static readonly UnitTestingInvocationReasons ProjectConfigurationChanged =
        new(
            [UnitTestingPredefinedInvocationReasons.ProjectConfigurationChanged, UnitTestingPredefinedInvocationReasons.SemanticChanged]);

    public static readonly UnitTestingInvocationReasons DocumentChanged =
        new(
            [UnitTestingPredefinedInvocationReasons.SemanticChanged]);

    public static readonly UnitTestingInvocationReasons AdditionalDocumentChanged =
        new(
            [UnitTestingPredefinedInvocationReasons.SemanticChanged]);

    public static readonly UnitTestingInvocationReasons SemanticChanged =
        new(
            [UnitTestingPredefinedInvocationReasons.SemanticChanged]);

    public static readonly UnitTestingInvocationReasons Reanalyze =
        new(UnitTestingPredefinedInvocationReasons.Reanalyze);
}
