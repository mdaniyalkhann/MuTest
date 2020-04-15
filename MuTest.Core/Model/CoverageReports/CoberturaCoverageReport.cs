using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace MuTest.Core.Model.CoverageReports
{
    public class Element
    {
        public double denominator { get; set; }
        public string name { get; set; }
        public double numerator { get; set; }
        public double ratio { get; set; }
    }


    public class Child
    {
        public List<Child> children { get; set; }
        public List<Element> elements { get; set; }
        public string name { get; set; }
    }


    public class Results
    {
        public List<Child> children { get; set; }
        public List<Element> elements { get; set; }
        public string name { get; set; }
    }

    public class CoberturaCoverageReport
    {
        public string _class { get; set; }
        public Results results { get; set; }
    }
}