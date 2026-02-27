namespace LoadSimulator.Utilities;

/// <summary>
/// Thread-safe mock data generator for load simulation
/// </summary>
public class MockDataGenerator
{
    private static readonly Lazy<MockDataGenerator> _instance = new(() => new MockDataGenerator());
    
    public static MockDataGenerator Instance => _instance.Value;
    
    private readonly object _lock = new object();
    
    private static readonly string[] FirstNames = new[]
    {
        "James", "Mary", "Robert", "Patricia", "Michael", "Jennifer",
        "William", "Linda", "David", "Barbara", "Richard", "Susan",
        "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen"
    };
    
    private static readonly string[] LastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia",
        "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez",
        "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore"
    };
    
    private static readonly string[] Domains = new[] { "gmail.com", "outlook.com", "yahoo.com", "example.com" };
    
    private static readonly int[] ProductIds = Enumerable.Range(1, 100).ToArray();

    public string GenerateEmail()
    {
        var firstName = FirstNames[Random.Shared.Next(FirstNames.Length)];
        var lastName = LastNames[Random.Shared.Next(LastNames.Length)];
        var domain = Domains[Random.Shared.Next(Domains.Length)];
        var randomNumber = Random.Shared.Next(10000);
        
        return $"{firstName.ToLower()}.{lastName.ToLower()}{randomNumber}@{domain}";
    }

    public string GeneratePassword() => 
        $"Password{Random.Shared.Next(100000, 999999)}!";

    public string GenerateUsername()
    {
        var firstName = FirstNames[Random.Shared.Next(FirstNames.Length)];
        var lastName = LastNames[Random.Shared.Next(LastNames.Length)];
        var randomNumber = Random.Shared.Next(1000, 9999);
        
        return $"{firstName.ToLower()}{lastName.ToLower()}{randomNumber}";
    }

    public int GenerateQuantity(int maxQuantity = 10) => 
        Math.Max(1, Random.Shared.Next(1, maxQuantity + 1));

    public int GenerateProductId() => 
        ProductIds[Random.Shared.Next(ProductIds.Length)];

    public List<int> GenerateProductIds(int count, int maxProductsPerOrder)
    {
        var productCount = Math.Min(count, maxProductsPerOrder);
        var selected = new HashSet<int>();
        
        while (selected.Count < productCount)
        {
            selected.Add(GenerateProductId());
        }
        
        return selected.ToList();
    }

    public TimeSpan GenerateThinkTime(int minMs, int maxMs) =>
        TimeSpan.FromMilliseconds(Random.Shared.Next(minMs, maxMs + 1));

    /// <summary>
    /// Generate think time using normal distribution (realistic user behavior)
    /// </summary>
    public TimeSpan GenerateNormalDistributionThinkTime(double mean, double stdDev)
    {
        var u1 = Random.Shared.NextDouble();
        var u2 = Random.Shared.NextDouble();
        
        var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        var thinkTime = (z * stdDev) + mean;
        
        return TimeSpan.FromMilliseconds(Math.Max(100, thinkTime));
    }

    public int GeneratePage(int maxPages = 10) => 
        Random.Shared.Next(1, maxPages + 1);

    public string GenerateFullName()
    {
        var firstName = FirstNames[Random.Shared.Next(FirstNames.Length)];
        var lastName = LastNames[Random.Shared.Next(LastNames.Length)];
        
        return $"{firstName} {lastName}";
    }
}
