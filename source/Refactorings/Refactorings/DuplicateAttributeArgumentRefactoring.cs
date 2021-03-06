﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class DuplicateAttributeArgumentRefactoring
    {
        public static void ComputeRefactoring(RefactoringContext context, AttributeArgumentListSyntax argumentList)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.DuplicateArgument))
            {
                AttributeArgumentSyntax argument = GetArgument(context, argumentList);

                if (argument != null)
                {
                    context.RegisterRefactoring(
                        "Duplicate argument",
                        cancellationToken => RefactorAsync(context.Document, argument, cancellationToken));
                }
            }
        }

        private static AttributeArgumentSyntax GetArgument(RefactoringContext context, AttributeArgumentListSyntax argumentList)
        {
            if (context.Span.IsEmpty)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> arguments = argumentList.Arguments;

                foreach (AttributeArgumentSyntax argument in arguments)
                {
                    if (argument.IsMissing
                        && context.Span.Contains(argument.Span))
                    {
                        int index = arguments.IndexOf(argument);

                        if (index > 0
                            && !arguments[index - 1].IsMissing)
                        {
                            return argument;
                        }
                    }
                }
            }

            return null;
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            AttributeArgumentSyntax argument,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var argumentList = (AttributeArgumentListSyntax)argument.Parent;

            int index = argumentList.Arguments.IndexOf(argument);

            AttributeArgumentSyntax previousArgument = argumentList.Arguments[index - 1]
                .WithTriviaFrom(argument);

            SyntaxNode newRoot = oldRoot.ReplaceNode(argument, previousArgument);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
