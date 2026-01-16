namespace Domain.Products;

public class Product
{
    public int Id { get; set; }
    public string Number { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }


    public string ManufacturePart { get; set; }
    public string ShortDesc { get; set; }
    public string Barcode { get; set; }
    public bool DLC { get; set; }
    public bool UL { get; set; }
    public bool ETL { get; set; }
    public bool ES { get; set; }
    public bool CUL { get; set; }
    public bool CETL { get; set; }
    public string IP { get; set; }
    public string SKU { get; set; }
    public string SPC { get; set; }
    public string ManufactorerPartNumber { get; set; }
    public int? IncomeAccountID { get; set; }
    public string Image { get; set; }
    public int? ReorderPoint { get; set; }

    public decimal? Price { get; set; }
    public bool Taxable { get; set; }
    public bool Salable { get; set; }
    public bool Enable { get; set; }
    public bool BatchTraceable { get; set; }
    public bool? Deleted { get; set; }
    public bool EccomerceViewable { get; set; }
    public bool EccomerceSalable { get; set; }
    public string Notes { get; set; }
    public float? WeightKg { get; set; }
    public float? LengthCm { get; set; }
    public float? WidthCm { get; set; }
    public float? HeightCm { get; set; }
    public string AlternateName { get; set; }


    public int? PreferredSupplierId { get; set; }
    public float MaximumStockLevel { get; set; }
    public float MinimumStockLevel { get; set; }
    public float PurchasePrice { get; set; }

}
