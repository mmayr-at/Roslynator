﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimplifyNestedUsingStatementCodeFixProvider))]
    [Shared]
    public class SimplifyNestedUsingStatementCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticIdentifiers.SimplifyNestedUsingStatement);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            UsingStatementSyntax usingStatement = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<UsingStatementSyntax>();

            if (usingStatement == null)
                return;

            bool fMultiple = usingStatement.Statement
                .DescendantNodes()
                .Any(f => f.IsKind(SyntaxKind.UsingStatement) && UsingStatementAnalysis.ContainsEmbeddableUsingStatement((UsingStatementSyntax)f));

            string title = "Remove braces from 'using' statement";

            if (fMultiple)
                title += "s";

            CodeAction codeAction = CodeAction.Create(
                title,
                cancellationToken =>
                {
                    return RemoveBracesFromUsingStatementRefactoring.RefactorAsync(
                        context.Document,
                        usingStatement,
                        cancellationToken);
                },
                DiagnosticIdentifiers.SimplifyNestedUsingStatement + EquivalenceKeySuffix);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
