﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator
{
    public static class SemanticModelExtensions
    {
        public static IMethodSymbol GetMethodSymbol(
            this SemanticModel semanticModel,
            ExpressionSyntax expression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol as IMethodSymbol;
        }
    }
}