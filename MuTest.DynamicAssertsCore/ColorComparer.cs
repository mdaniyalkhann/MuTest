using System;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace MuTest.DynamicAsserts
{
    public class ColorComparer : BaseTypeComparer
    {
        private const string ColorType = "System.Drawing.Color";

        public ColorComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1.FullName == ColorType;
        }

        public override void CompareType(CompareParms parms)
        {
            if (!parms.Object1.Equals(parms.Object2))
            {
                AddDifference(parms);
            }
        }
    }
}