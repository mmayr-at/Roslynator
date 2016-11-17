﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class IntroduceLocalFromStatementThatReturnsValueRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, ExpressionStatementSyntax expressionStatement)
        {
            ExpressionSyntax expression = expressionStatement.Expression;

            if (expression?.IsMissing == false)
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, context.CancellationToken);

                if (typeSymbol?.IsErrorType() == false
                    && !typeSymbol.IsVoid())
                {
                    context.RegisterRefactoring(
                        $"Introduce local for '{expression}'",
                        cancellationToken => RefactorAsync(context.Document, expressionStatement, typeSymbol, cancellationToken));
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ExpressionStatementSyntax expressionStatement,
            ITypeSymbol typeSymbol,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            string identifier = SyntaxUtility.CreateIdentifier(typeSymbol, firstCharToLower: true) ?? "x";

            identifier = SyntaxUtility.GetUniqueName(identifier, semanticModel, expressionStatement.Span.Start);

            LocalDeclarationStatementSyntax newNode = LocalDeclarationStatement(
                Var(),
                Identifier(identifier).WithRenameAnnotation(),
                expressionStatement.Expression);

            newNode = newNode
                .WithTriviaFrom(expressionStatement)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(expressionStatement, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}