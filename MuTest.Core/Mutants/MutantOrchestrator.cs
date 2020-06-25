using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutators;
using MuTest.Core.Utility;

namespace MuTest.Core.Mutants
{
    public interface IMutantOrchestrator
    {
        SyntaxNode Mutate(SyntaxNode rootNode);

        IEnumerable<Mutant> GetLatestMutantBatch();
    }

    public class MutantOrchestrator : IMutantOrchestrator
    {
        private ICollection<Mutant> Mutants { get; set; }
        private int MutantCount { get; set; }
        private IEnumerable<IMutator> Mutators { get; }

        public MutantOrchestrator(IEnumerable<IMutator> mutators = null)
        {
            Mutators = mutators ?? new List<IMutator>
            {
                new AssignmentStatementMutator(),
                new ArithmeticOperatorMutator(),
                new RelationalOperatorMutator(),
                new LogicalConnectorMutator(),
                new StatementBlockMutator(),
                new BitwiseOperatorMutator(),
                new BooleanMutator(),
                new CheckedMutator(),
                new InterpolatedStringMutator(),
                new LinqMutator(),
                new MethodCallMutator(),
                new NegateConditionMutator(),
                new NonVoidMethodCallMutator(),
                new PostfixUnaryMutator(),
                new PrefixUnaryMutator(),
                new StringMutator()
            };
            Mutants = new Collection<Mutant>();
        }

        public static IEnumerable<Mutant> GetDefaultMutants(SyntaxNode node)
        {
            var orchestrator = new MutantOrchestrator(new List<IMutator>
            {
                new AssignmentStatementMutator(),
                new ArithmeticOperatorMutator(),
                new LogicalConnectorMutator(),
                new RelationalOperatorMutator(),
                new InterpolatedStringMutator(),
                new StringMutator(),
                new StatementBlockMutator(),
                new MethodCallMutator()
            });

            orchestrator.Mutate(node);

            return orchestrator.GetLatestMutantBatch();
        }

        public IEnumerable<Mutant> GetLatestMutantBatch()
        {
            var mutants = new List<Mutant>();
            foreach (var mutant in Mutants)
            {
                if (mutant.Mutation.Type != MutatorType.MethodCall)
                {
                    mutants.Add(mutant);
                    continue;
                }

                if (Mutants.Count(x => x.Mutation.Location == mutant.Mutation.Location) == 1)
                {
                    mutants.Add(mutant);
                }
            }

            var tempMutants = mutants;
            Mutants = new Collection<Mutant>();
            return tempMutants;
        }

        public SyntaxNode Mutate(SyntaxNode currentNode)
        {
            if (GetExpressionSyntax(currentNode) is var expressionSyntax && expressionSyntax != null)
            {
                if (currentNode is ExpressionStatementSyntax syntax)
                {
                    if (expressionSyntax is AssignmentExpressionSyntax)
                    {
                        return MutateWithIfStatements(expressionSyntax.Parent);
                    }

                    if (GetExpressionSyntax(expressionSyntax) is var subExpressionSyntax && subExpressionSyntax != null)
                    {

                        return currentNode.ReplaceNode(expressionSyntax, Mutate(expressionSyntax));
                    }

                    return MutateWithIfStatements(syntax);
                }

                return currentNode.ReplaceNode(expressionSyntax, MutateWithConditionalExpressions(expressionSyntax));
            }

            if (currentNode is StatementSyntax statement && currentNode.Kind() != SyntaxKind.Block)
            {
                if (currentNode is LocalFunctionStatementSyntax localFunction)
                {
                    return localFunction.ReplaceNode(localFunction.Body, Mutate(localFunction.Body));
                }

                if (currentNode is IfStatementSyntax ifStatement)
                {
                    if (!ifStatement.Statement.ChildNodes().Any())
                    {
                        return null;
                    }

                    ifStatement = ifStatement.ReplaceNode(ifStatement.Condition, MutateWithConditionalExpressions(ifStatement.Condition));

                    if (ifStatement.Else != null)
                    {
                        ifStatement = ifStatement.ReplaceNode(ifStatement.Else, Mutate(ifStatement.Else));
                    }

                    try
                    {
                        if (ifStatement.Statement != null)
                        {
                            return ifStatement.ReplaceNode(ifStatement.Statement, Mutate(ifStatement.Statement));
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("unable to process if statement at line {0} {1}", ifStatement?.Statement?.LineNumber() + 1, e);
                    }
                }

                return MutateWithIfStatements(statement);
            }

            if (currentNode is InvocationExpressionSyntax invocationExpression && invocationExpression.ArgumentList.Arguments.Count == 0)
            {
                var mutant = FindMutants(invocationExpression).FirstOrDefault();
                if (mutant != null)
                {
                    Mutants.Add(mutant);
                }
            }

            AddBlockMutants(currentNode);

            var children = currentNode.ChildNodes().ToList();
            foreach (var child in children)
            {
                Mutate(child);
            }

            return currentNode;
        }

        private void AddBlockMutants(SyntaxNode currentNode)
        {
            if (currentNode is StatementSyntax block && currentNode.Kind() == SyntaxKind.Block)
            {
                var mutant = FindMutants(block).FirstOrDefault();
                if (mutant != null)
                {
                    Mutants.Add(mutant);
                }
            }
        }

        private IEnumerable<Mutant> FindMutants(SyntaxNode current)
        {
            foreach (var mutator in Mutators)
            {
                foreach (var mutation in ApplyMutator(current, mutator))
                {
                    yield return mutation;
                }
            }

            foreach (var mutant in current.ChildNodes().SelectMany(FindMutants))
            {
                yield return mutant;
            }
        }

        private SyntaxNode MutateWithIfStatements(SyntaxNode currentNode)
        {
            var ast = currentNode;
            foreach (var mutant in currentNode.ChildNodes().SelectMany(FindMutants))
            {
                Mutants.Add(mutant);
            }

            return ast;
        }

        private SyntaxNode MutateWithConditionalExpressions(ExpressionSyntax currentNode)
        {
            ExpressionSyntax expressionAst = currentNode;
            foreach (var mutant in FindMutants(currentNode))
            {
                Mutants.Add(mutant);
            }

            return expressionAst;
        }

        private IEnumerable<Mutant> ApplyMutator(SyntaxNode syntaxNode, IMutator mutator)
        {
            var mutations = mutator.Mutate(syntaxNode);
            foreach (var mutation in mutations)
            {
                yield return new Mutant
                {
                    Id = MutantCount++,
                    Mutation = mutation,
                    ResultStatus = MutantStatus.NotRun
                };
            }
        }

        private ExpressionSyntax GetExpressionSyntax(SyntaxNode node)
        {
            switch (node.GetType().Name)
            {
                case nameof(LocalDeclarationStatementSyntax):
                    var localDeclarationStatement = node as LocalDeclarationStatementSyntax;
                    return localDeclarationStatement?.Declaration.Variables.First().Initializer?.Value;
                case nameof(AssignmentExpressionSyntax):
                    var assignmentExpression = node as AssignmentExpressionSyntax;
                    return assignmentExpression?.Right;
                case nameof(ReturnStatementSyntax):
                    var returnStatement = node as ReturnStatementSyntax;
                    return returnStatement?.Expression;
                case nameof(LocalFunctionStatementSyntax):
                    var localFunction = node as LocalFunctionStatementSyntax;
                    return localFunction?.ExpressionBody?.Expression;
                case nameof(ExpressionStatementSyntax):
                    var expressionStatement = node as ExpressionStatementSyntax;
                    return expressionStatement?.Expression;
                default:
                    return null;
            }
        }
    }
}