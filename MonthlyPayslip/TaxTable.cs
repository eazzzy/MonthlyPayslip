using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: CLSCompliant(true)]
namespace MonthlyPaySlip
{
  public class TaxTable
  {
    private const int MonthsInYear = 12;

    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Tax { get; set; }
    public decimal AdditionalCharge { get; set; }
    public IEnumerable<TaxTable> TaxTableEntries { get; set; }

    public decimal GetMonthlyIncomeTaxFromAnnualSalary(decimal annualSalary)
    {
      var entry = GetTaxTableEntryFromAnnualSalary(annualSalary);

      return ((entry.Tax + (annualSalary - entry.Min) * entry.AdditionalCharge) / MonthsInYear).RoundToWholeNumber();
    }

    internal TaxTable GetTaxTableEntryFromAnnualSalary(decimal annualSalary)
    {
      ValidateTaxTableEntries();

      var entry = TaxTableEntries.Where(i => i.Min <= annualSalary).LastOrDefault();

      if (entry != null)
        return entry;
      else
        throw new KeyNotFoundException("Could not find a tax entry matching the input salary");
    }

    private void ValidateTaxTableEntries()
    {
      if (TaxTableEntries == null)
        throw new InvalidOperationException("Tax table entries is null or has not been loaded");
    }

    public IEnumerator<TaxTable> GetEnumerator()
    {
      return TaxTableEntries.GetEnumerator();
    }
  }

  [TestFixture]
  public static class TaxTableTest
  {
    [TestCase(1)]
    public static void TestTaxTableNotLoadedExceptionHandling(decimal annualSalary)
    {
      var taxTable = new TaxTable();
      
      Assert.Throws<InvalidOperationException>(() => taxTable.GetTaxTableEntryFromAnnualSalary(annualSalary));
    }

    [TestCase(-1)]
    [TestCase(17995)]
    [TestCase(60050)]
    public static void TestTaxTableEntryNotFoundExceptionHandling(decimal annualSalary)
    {
      TaxTable taxTable = new TaxTable();

      var taxTableEntries = LoadTaxTableTestData().ElementAtOrDefault(3);

      taxTable.TaxTableEntries = new List<TaxTable>() { taxTableEntries };
      
      Assert.Throws<KeyNotFoundException>(() => taxTable.GetTaxTableEntryFromAnnualSalary(annualSalary));
    }

    [TestCase(17995, 0)]
    [TestCase(18200, 0)]
    [TestCase(18235, 1)]
    [TestCase(25000, 108)]
    [TestCase(60050, 922)]
    [TestCase(85000, 1616)]
    [TestCase(120000, 2696)]
    [TestCase(190000, 4921)]
    public static void TestTaxCalculationFromAnnualSalaryProducesExpectedOutcome(decimal annualSalary, decimal expectedIncomeTax)
    {
      TaxTable taxTable = new TaxTable();

      taxTable.TaxTableEntries = LoadTaxTableTestData();

      var incomeTax = taxTable.GetMonthlyIncomeTaxFromAnnualSalary(annualSalary);

      Assert.AreEqual(expectedIncomeTax, incomeTax);
    }

    [TestCase(17995)]
    [TestCase(18200)]
    [TestCase(18235)]
    [TestCase(25000)]
    [TestCase(60050)]
    [TestCase(85000)]
    [TestCase(120000)]
    [TestCase(190000)]
    public static void TestTaxTableEntryFromAnnualSalaryHasExpectedOutcome(decimal annualSalary)
    {
      TaxTable taxTable = new TaxTable();

      taxTable.TaxTableEntries = LoadTaxTableTestData();

      var taxEntry = taxTable.GetTaxTableEntryFromAnnualSalary(annualSalary);

      Assert.IsTrue(Enumerable.Range((int)taxEntry.Min, (int)(taxEntry.Max - taxEntry.Min) + 1).Contains((int)annualSalary));
    }

    private static IEnumerable<TaxTable> LoadTaxTableTestData()
    {
      var taxEntries = new List<TaxTable>();

      taxEntries.Add(new TaxTable() { Min = 0, Max = 18200, Tax = 0, AdditionalCharge = 0 });
      taxEntries.Add(new TaxTable() { Min = 18201, Max = 37000, Tax = 0, AdditionalCharge = (decimal)0.19 });
      taxEntries.Add(new TaxTable() { Min = 37001, Max = 80000, Tax = 3572, AdditionalCharge = (decimal)0.325 });
      taxEntries.Add(new TaxTable() { Min = 80001, Max = 180000, Tax = 17547, AdditionalCharge = (decimal)0.37 });
      taxEntries.Add(new TaxTable() { Min = 180001, Max = 2147483647, Tax = 54547, AdditionalCharge = (decimal)0.45 });

      return taxEntries;
    }
  }
}
