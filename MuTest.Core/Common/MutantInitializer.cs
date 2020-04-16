﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Model;
using MuTest.Core.Mutants;
using MuTest.Core.Mutators;
using MuTest.Core.Utility;

namespace MuTest.Core.Common
{
    public class MutantInitializer : IMutantInitializer
    {
        private readonly SourceClassDetail _source;

        public string MutantFilterId { get; set; }

        public string MutantFilterRegEx { get; set; }

        public bool ExecuteAllTests { get; set; } = false;

        public string SpecificFilterRegEx { get; set; }

        public List<int> MutantsAtSpecificLines { get; } = new List<int>();

        public MutantInitializer(SourceClassDetail source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public async Task InitializeMutants(IList<object> selectedMutators)
        {
            if (selectedMutators == null || !selectedMutators.Any())
            {
                return;
            }

            var mutatorFinder = new MutantOrchestrator(selectedMutators.Cast<IMutator>());
            var id = 1;
            foreach (var method in _source.MethodDetails)
            {
                method.TestMethods.Clear();
                method.Mutants.Clear();
                mutatorFinder.Mutate(method.Method);
                var latestMutantBatch = mutatorFinder.GetLatestMutantBatch().ToList();
                foreach (var mutant in latestMutantBatch)
                {
                    mutant.Method = method;
                }

                method.Mutants.AddRange(latestMutantBatch);

                foreach (var mutant in method.Mutants)
                {
                    mutant.Id = id++;
                }

                FilterMutants(method);

                await Task.Run(() =>
                {
                    foreach (var testMethod in _source.TestClaz.MethodDetails)
                    {
                        if (method.Coverage?.LinesCovered == 0)
                        {
                            break;
                        }

                        var sourceMethodName = method.Method.MethodName();
                        var className = method.Method.Class().ClassName();
                        if (!ExecuteAllTests && !method.IsProperty)
                        {
                            if (testMethod.Method.ValidTestMethod(className, sourceMethodName, _source.TestClaz.Claz))
                            {
                                method.TestMethods.Add(testMethod);
                                continue;
                            }

                            var methods = method.Method.Class().Methods();
                            if (methods != null)
                            {
                                foreach (var classMethod in methods)
                                {
                                    var methodName = classMethod.MethodName();
                                    if (classMethod.ChildMethodNames().Any(x => x.Contains(sourceMethodName)) &&
                                        testMethod.Method.ValidTestMethod(className, methodName, _source.TestClaz.Claz))
                                    {
                                        method.TestMethods.Add(testMethod);
                                        if (!method.ParentMethodNames.Contains(methodName))
                                        {
                                            method.ParentMethodNames.Add(methodName);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            method.TestMethods.Add(testMethod);
                        }
                    }

                    if (!method.TestMethods.Any() && method.Coverage?.LinesCovered > 0)
                    {
                        method.TestMethods.AddRange(_source.TestClaz.MethodDetails);
                    }
                });

                foreach (var sourceMethod in _source.MethodDetails)
                {
                    var parentMethods = _source
                        .MethodDetails
                        .Where(x => sourceMethod.ParentMethodNames.Contains(x.Method.MethodName())).ToList();

                    foreach (MethodDetail parentMethod in parentMethods)
                    {
                        foreach (var testMethod in parentMethod.TestMethods)
                        {
                            if (sourceMethod.TestMethods.All(x => x.Method.MethodName() != testMethod.Method.MethodName()))
                            {
                                sourceMethod.TestMethods.Add(testMethod);
                            }
                        }
                    }
                }

                var uncoveredLines = new List<CoverageDSPriv.LinesRow>();
                var containTests = method.TestMethods.Any();
                if (containTests)
                {
                    uncoveredLines = method
                        .Lines
                        .Where(x => x.Coverage > 0).ToList();
                }

                foreach (var mutant in method.Mutants)
                {
                    if (containTests &&
                        uncoveredLines.Any())
                    {
                        if (uncoveredLines.Any(x => x.LnStart == mutant.Mutation.Location))
                        {
                            mutant.ResultStatus = MutantStatus.NotCovered;
                        }
                    }

                    if (!containTests)
                    {
                        mutant.ResultStatus = MutantStatus.NotCovered;
                    }
                }
            }
        }

        private void FilterMutants(MethodDetail method)
        {
            if (MutantsAtSpecificLines.Any())
            {
                foreach (var mutant in method.Mutants)
                {
                    if (!MutantsAtSpecificLines.Contains(mutant.Mutation.Location.GetValueOrDefault()))
                    {
                        mutant.ResultStatus = MutantStatus.Skipped;
                    }
                }

                return;
            }

            var idList = new List<string>();
            if (!string.IsNullOrWhiteSpace(MutantFilterId))
            {
                if (MutantFilterId.Contains(','))
                {
                    idList.AddRange(MutantFilterId.Split(','));
                }
                else
                {
                    idList.Add(MutantFilterId.Trim());
                }
            }

            foreach (var key in idList)
            {
                var mutant = method.Mutants.FirstOrDefault(x => x.Id.ToString().Equals(key));
                if (mutant != null)
                {
                    mutant.ResultStatus = MutantStatus.Skipped;
                }
            }

            var regExList = new List<string>();
            if (!string.IsNullOrWhiteSpace(MutantFilterRegEx))
            {
                if (MutantFilterRegEx.Contains(','))
                {
                    regExList.AddRange(MutantFilterRegEx.Split(','));
                }
                else
                {
                    regExList.Add(MutantFilterRegEx);
                }
            }

            var spRegExList = new List<string>();
            if (!string.IsNullOrWhiteSpace(SpecificFilterRegEx))
            {
                if (SpecificFilterRegEx.Contains(','))
                {
                    spRegExList.AddRange(SpecificFilterRegEx.Split(','));
                }
                else
                {
                    spRegExList.Add(SpecificFilterRegEx);
                }
            }

            foreach (var regEx in regExList)
            {
                method.Mutants.ForEach(x =>
                {
                    if (x.ResultStatus != MutantStatus.Skipped)
                    {
                        if (Regex.IsMatch(x.Mutation.OriginalNode.ToString(), regEx))
                        {
                            x.ResultStatus = MutantStatus.Skipped;
                        }
                        else
                        {
                            var parentInvocation = x
                                .Mutation
                                .OriginalNode
                                .Ancestors<InvocationExpressionSyntax>().FirstOrDefault();

                            var objInitializer = x
                                .Mutation
                                .OriginalNode
                                .Ancestors<ObjectCreationExpressionSyntax>().FirstOrDefault();

                            if (parentInvocation != null && Regex.IsMatch(parentInvocation.ToString(), regEx) ||
                                objInitializer != null && Regex.IsMatch(objInitializer.ToString(), regEx))
                            {
                                x.ResultStatus = MutantStatus.Skipped;
                            }
                        }
                    }
                });
            }

            foreach (var regEx in spRegExList)
            {
                method.Mutants.ForEach(x =>
                {
                    if (x.ResultStatus != MutantStatus.Skipped)
                    {
                        var parentInvocation = x
                            .Mutation
                            .OriginalNode
                            .Ancestors<InvocationExpressionSyntax>().FirstOrDefault();

                        var objInitializer = x
                            .Mutation
                            .OriginalNode
                            .Ancestors<ObjectCreationExpressionSyntax>().FirstOrDefault();

                        if (!(parentInvocation != null && Regex.IsMatch(parentInvocation.ToString(), regEx) ||
                            objInitializer != null && Regex.IsMatch(objInitializer.ToString(), regEx) ||
                            Regex.IsMatch(x.Mutation.OriginalNode.ToString(), regEx)))
                        {
                            x.ResultStatus = MutantStatus.Skipped;
                        }
                    }
                });
            }
        }
    }
}