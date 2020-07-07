using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Common;
using MuTest.Core.Mutants;
using MuTest.Core.Mutators;
using MuTest.Core.Tests.Utility;
using MuTest.Core.Utility;
using NUnit.Framework;
using Shouldly;

namespace MuTest.Core.Tests
{
    /// <summary>
    /// <see cref="MutantSelector"/>
    /// </summary>
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class MutantSelectorTests
    {
        private const string SampleClassRelativePath = @"Samples\MutantSelectorSampleClass.cs";
        private const string MethodWithSingleMutantLcrPerline = "Method_With_Single_Mutant_LCR_PerLine";
        private const string MethodWithTwoMutantsLcrRorPerLine = "Method_With_Two_Mutants_LCR_ROR_Per_Line";
        private const string  MethodWithTwoMutantsRorUorPerLine = "Method_With_Two_Mutants_ROR_UOR_Per_Line";
        private const string  MethodWithTwoMutantsSbrAorPerLine = "Method_With_Two_Mutants_SBR_AOR_Per_Line";
        private const string  MethodWithThreeMutantsSbrUorAorPerLine = "Method_With_Three_Mutants_SBR_UOR_AOR_Per_Line";
        private const string  MethodWithFiveMutantsSbrLcrRorUorAorPerLine = "Method_With_Five_Mutants_SBR_LCR_ROR_UOR_AOR_Per_Line";
        private const string  MethodWithOneMutantVmcrPerLine = "Method_With_One_Mutant_VMCR_Per_Line";
        private const string  MethodWithTwoMutantVmcrSlrPerLine = "Method_With_Two_Mutant_VMCR_SLR_Per_Line";
        private MutantSelector _mutantSelector;
        private ClassDeclarationSyntax _class;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _class = SampleClassRelativePath.GetSampleClassDeclarationSyntax();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _class = null;
        }

        [SetUp]
        public void SetUp()
        {
            _mutantSelector = new MutantSelector();
        }

        [Test]
        public void SelectMutants_Method_With_Single_Mutant_LCR_PerLine_ShouldSelect_LCR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithSingleMutantLcrPerline);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(1, mutants);

            // Assert
            bool LineWithLcr(Mutant x) => x.Mutation.Location == mutants.First(m => m.Mutation.Type == MutatorType.Logical).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithLcr).ShouldBe(2),
                () => selectedMutants.Count(LineWithLcr).ShouldBe(1));

            selectedMutants.First(LineWithLcr).Mutation.Type.ShouldBe(MutatorType.Logical);
        }

        [Test]
        public void SelectMutants_Method_With_Two_Mutants_LCR_ROR_Per_Line_ShouldSelect_LCR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantsLcrRorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(1, mutants);

            // Assert
            bool LineWithLcr(Mutant x) => x.Mutation.Location == mutants.First(m => m.Mutation.Type == MutatorType.Logical).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithLcr).ShouldBe(6),
                () => selectedMutants.Count(LineWithLcr).ShouldBe(1));

            selectedMutants.First(LineWithLcr).Mutation.Type.ShouldBe(MutatorType.Logical);
        }

        [Test]
        public void SelectMutants_Method_With_Two_Mutants_LCR_ROR_Per_Line_Select_Two_ShouldSelect_LCR_ROR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantsLcrRorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(2, mutants);

            // Assert
            bool LineWithLcr(Mutant x) => x.Mutation.Location == mutants.First(m => m.Mutation.Type == MutatorType.Logical).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithLcr).ShouldBe(6),
                () => selectedMutants.Count(LineWithLcr).ShouldBe(2));

            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First(LineWithLcr).Mutation.Type.ShouldBe(MutatorType.Logical),
                () => selectedMutants.Last(LineWithLcr).Mutation.Type.ShouldBe(MutatorType.Relational));
        }

        [Test]
        public void SelectMutants_Method_With_Two_Mutants_ROR_UOR_Per_Line_ShouldSelect_UOR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantsRorUorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(1, mutants);

            // Assert
            bool LineWithUor(Mutant x) => x.Mutation.Location == mutants.First(m => m.Mutation.Type == MutatorType.Unary).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithUor).ShouldBe(3),
                () => selectedMutants.Count(LineWithUor).ShouldBe(1));

            selectedMutants.First(LineWithUor).Mutation.Type.ShouldBe(MutatorType.Unary);
        }

        [Test]
        public void SelectMutants_Method_With_Two_Mutants_ROR_UOR_Per_Line_Select_Two_ShouldSelect_UOR_ROR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantsRorUorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(2, mutants);

            // Assert
            bool LineWithUor(Mutant x) => x.Mutation.Location == mutants.First(m => m.Mutation.Type == MutatorType.Unary).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithUor).ShouldBe(3),
                () => selectedMutants.Count(LineWithUor).ShouldBe(2));

            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First(LineWithUor).Mutation.Type.ShouldBe(MutatorType.Unary),
                () => selectedMutants.Last(LineWithUor).Mutation.Type.ShouldBe(MutatorType.Relational));
        }

        [Test]
        public void SelectMutants_Method_With_Two_Mutants_SBR_AOR_Per_Line_ShouldSelect_SBR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantsSbrAorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(1, mutants);

            // Assert
            bool LineWithSbr(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.Block).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithSbr).ShouldBe(2),
                () => selectedMutants.Count(LineWithSbr).ShouldBe(1));

            selectedMutants.First(LineWithSbr).Mutation.Type.ShouldBe(MutatorType.Block);
        }

        [Test]
        public void SelectMutants_Method_With_Two_Mutants_SBR_AOR_Per_Line_Select_Two_ShouldSelect_SBR_AOR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantsSbrAorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(2, mutants);

            // Assert
            bool LineWithSbr(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.Block).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithSbr).ShouldBe(2),
                () => selectedMutants.Count(LineWithSbr).ShouldBe(2));

            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First(LineWithSbr).Mutation.Type.ShouldBe(MutatorType.Block),
                () => selectedMutants.Last(LineWithSbr).Mutation.Type.ShouldBe(MutatorType.Arithmetic));
        }

        [Test]
        public void SelectMutants_Method_With_Three_Mutants_SBR_UOR_AOR_Per_Line_Select_Two_ShouldSelect_UOR_SBR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithThreeMutantsSbrUorAorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(2, mutants);

            // Assert
            bool LineWithSbr(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.Block).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithSbr).ShouldBe(3),
                () => selectedMutants.Count(LineWithSbr).ShouldBe(2));

            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First(LineWithSbr).Mutation.Type.ShouldBe(MutatorType.Unary),
                () => selectedMutants.Last(LineWithSbr).Mutation.Type.ShouldBe(MutatorType.Block));
        }

        [Test]
        public void SelectMutants_Method_With_Three_Mutants_SBR_UOR_AOR_Per_Line_Select_Three_ShouldSelect_UOR_SBR_AOR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithThreeMutantsSbrUorAorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(3, mutants);

            // Assert
            bool LineWithSbr(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.Block).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithSbr).ShouldBe(3),
                () => selectedMutants.Count(LineWithSbr).ShouldBe(3));

            selectedMutants = selectedMutants.Where(LineWithSbr).ToList();
            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First().Mutation.Type.ShouldBe(MutatorType.Unary),
                () => selectedMutants.Skip(1).First().Mutation.Type.ShouldBe(MutatorType.Block),
                () => selectedMutants.Last().Mutation.Type.ShouldBe(MutatorType.Arithmetic));
        }

        [Test]
        public void Method_With_Five_Mutants_SBR_LCR_ROR_UOR_AOR_Per_Line_Select_Three_ShouldSelect_LCR_UOR_ROR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithFiveMutantsSbrLcrRorUorAorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(3, mutants);

            // Assert
            bool LineWithLcr(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.Logical).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithLcr).ShouldBe(9),
                () => selectedMutants.Count(LineWithLcr).ShouldBe(3));

            selectedMutants = selectedMutants.Where(LineWithLcr).ToList();
            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First().Mutation.Type.ShouldBe(MutatorType.Logical),
                () => selectedMutants.Skip(1).First().Mutation.Type.ShouldBe(MutatorType.Unary),
                () => selectedMutants.Last().Mutation.Type.ShouldBe(MutatorType.Relational));
        }

        [Test]
        public void Method_With_Five_Mutants_SBR_LCR_ROR_UOR_AOR_Per_Line_Select_Seven_ShouldSelect_LCR_UOR_ROR_ROR_ROR_ROR_SBR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithFiveMutantsSbrLcrRorUorAorPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(7, mutants);

            // Assert
            bool LineWithLcr(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.Logical).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count(LineWithLcr).ShouldBe(9),
                () => selectedMutants.Count(LineWithLcr).ShouldBe(7));

            selectedMutants = selectedMutants.Where(LineWithLcr).ToList();
            this.ShouldSatisfyAllConditions(
                () => selectedMutants.First().Mutation.Type.ShouldBe(MutatorType.Logical),
                () => selectedMutants.Skip(1).First().Mutation.Type.ShouldBe(MutatorType.Unary),
                () => selectedMutants.Skip(2).First().Mutation.Type.ShouldBe(MutatorType.Relational),
                () => selectedMutants.Skip(3).First().Mutation.Type.ShouldBe(MutatorType.Relational),
                () => selectedMutants.Skip(4).First().Mutation.Type.ShouldBe(MutatorType.Relational),
                () => selectedMutants.Skip(5).First().Mutation.Type.ShouldBe(MutatorType.Relational),
                () => selectedMutants.Last().Mutation.Type.ShouldBe(MutatorType.Block));
        }

        [Test]
        public void Method_With_One_Mutant_VMCR_Per_Line_ShouldSelect_VMCR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithOneMutantVmcrPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(1, mutants);

            // Assert
            bool LineWithVoidMethodCall(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.MethodCall).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count.ShouldBe(2),
                () => selectedMutants.Count(LineWithVoidMethodCall).ShouldBe(1));

            selectedMutants.First(LineWithVoidMethodCall).Mutation.Type.ShouldBe(MutatorType.MethodCall);
        }

        [Test]
        public void Method_With_Two_Mutant_VMCR_SLR_Per_Line_ShouldSelect_VMCR()
        {
            // Arrange
            var mutants = GetMethodMutants(MethodWithTwoMutantVmcrSlrPerLine);

            // Act
            var selectedMutants = _mutantSelector.SelectMutants(1, mutants);

            // Assert
            bool LineWithVoidMethodCall(Mutant x) => x.Mutation.Location == mutants.Last(m => m.Mutation.Type == MutatorType.String).Mutation.Location;
            this.ShouldSatisfyAllConditions(
                () => mutants.Count.ShouldBe(2),
                () => selectedMutants.Count(LineWithVoidMethodCall).ShouldBe(1));

            selectedMutants.First(LineWithVoidMethodCall).Mutation.Type.ShouldBe(MutatorType.String);
        }

        private IList<Mutant> GetMethodMutants(string method)
        {
            var methodSyntax = _class
                .DescendantNodes<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.MethodName() == method);
            if (methodSyntax != null)
            {
                var mutantOrchestrator = new MutantOrchestrator();
                mutantOrchestrator.Mutate(methodSyntax);

                return mutantOrchestrator.GetLatestMutantBatch().ToList();
            }

            return new List<Mutant>();
        }
    }
}
