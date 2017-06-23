using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonthlyPaySlip
{
  public class TaxTable
  {
    readonly private IEnumerable<TaxTable> TaxTableEntries;

    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Tax { get; set; }
    public decimal AdditionalCharge { get; set; }

    public TaxTable() { }

    public TaxTable(IEnumerable<TaxTable> taxTableEntries)
    {
      TaxTableEntries = taxTableEntries;
    }

    public TaxTable GetTaxTableEntryFromAnnualSalary(decimal annualSalary)
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
  }

  public class PaySlipCalculator
  {
    private const int MonthsInYear = 12;

    public decimal CalculateIncomeTaxFromAnnualSalary(decimal annualSalary, TaxTable taxTableEntry)
    {
       return ((taxTableEntry.Tax + (annualSalary - taxTableEntry.Min) * taxTableEntry.AdditionalCharge) / MonthsInYear).RoundToWholeNumber();
    }
  }

  public static class Utilities
  {
    public static IEnumerable<TaxTable> LoadTaxTableTestData()
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

  [TestFixture]
  public class TaxTableTest
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

      var taxTableEntries = Utilities.LoadTaxTableTestData().ElementAtOrDefault(3);
      TaxTable taxTable = new TaxTable(new List<TaxTable>() { taxTableEntries });

      Assert.Throws<KeyNotFoundException>(() => taxTable.GetTaxTableEntryFromAnnualSalary(annualSalary));
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

      var taxTableEntries = Utilities.LoadTaxTableTestData();

      TaxTable taxTable = new TaxTable(taxTableEntries);

      var taxEntry = taxTable.GetTaxTableEntryFromAnnualSalary(annualSalary);

      Assert.IsTrue(Enumerable.Range((int)taxEntry.Min, (int)(taxEntry.Max - taxEntry.Min) + 1).Contains((int)annualSalary));
    }
  }

  [TestFixture]
  public class PaySlipCalculatorTest
  {
    [TestCase(17995, 0)]
    [TestCase(18200, 0)]
    [TestCase(18235, 1)]
    [TestCase(25000, 108)]
    [TestCase(60050, 922)]
    [TestCase(85000, 1616)]
    [TestCase(120000, 2696)]
    [TestCase(190000, 4921)]
    public static void TestPaySlipCalculatorFromAnnualSalaryProducesExpectedOutcome(decimal annualSalary, decimal expectedIncomeTax)
    {

      var taxTableEntries = Utilities.LoadTaxTableTestData();

      TaxTable taxTable = new TaxTable(taxTableEntries);

      var taxEntry = taxTable.GetTaxTableEntryFromAnnualSalary(annualSalary);

      var payslipCalc = new PaySlipCalculator();

      var incomeTax = payslipCalc.CalculateIncomeTaxFromAnnualSalary(annualSalary, taxEntry);

      Assert.AreEqual(expectedIncomeTax, incomeTax);
    }
  }
}
