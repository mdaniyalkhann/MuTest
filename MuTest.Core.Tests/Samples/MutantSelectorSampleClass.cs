// ReSharper disable UnusedMember.Global

using System;
using System.Diagnostics.CodeAnalysis;

namespace MuTest.Core.Tests.Samples
{
    [ExcludeFromCodeCoverage]
    public class MutantSelectorSampleClass
    {
        public int Method_With_Single_Mutant_LCR_PerLine(bool x, bool y)
        {
            if (x && y) // LCR
            {
                return 1;
            }

            return 0;
        }

        public int Method_With_Two_Mutants_LCR_ROR_Per_Line(int x, int y)
        {
            if (x > 0 && y < 0) // LCR and ROR
            {
                return 1;
            }

            return 0;
        }

        public int Method_With_Two_Mutants_ROR_UOR_Per_Line(int x, int y)
        {
            for (var index = 0; index < y; index++) // ROR and UOR
            {
                if (index == 1) // ROR
                {
                    return 10;
                }
            }

            return 0;
        }

        public int Method_With_Two_Mutants_SBR_AOR_Per_Line(int x, int y)
        {
            { return x + y; } // SBR and AOR
        }

        public int Method_With_Three_Mutants_SBR_UOR_AOR_Per_Line(int x, int y)
        {
            { return ++x + y; } // SBR UOR and AOR
        }

        public bool Method_With_Five_Mutants_SBR_LCR_ROR_UOR_AOR_Per_Line(int x, int y)
        {
            { return x + y > 8 || ++x * y > 1; } // SBR LCR ROR UOR and AOR
        }

        public void Method_With_One_Mutant_VMCR_Per_Line()
        {
            Method_With_Three_Mutants_SBR_UOR_AOR_Per_Line(1, 2); // Void Method Call
        }

        public void Method_With_Two_Mutant_VMCR_SLR_Per_Line()
        {
            Environment.GetEnvironmentVariable("Testing");
        }

        public void Method_With_No_Mutants_As_Arid()
        {
            Console.WriteLine("Testing");
        }
    }
}