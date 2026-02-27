namespace LoadSimulator.Models.DTOs;

/// <summary>
/// Represents a simulated user session
/// </summary>
public class UserSessionDto
{
    public string UserId { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string JwtToken { get; set; } = string.Empty;
    
    public DateTime LoginTime { get; set; }
    
    public DateTime TokenExpiryTime { get; set; }
    
    public bool IsTokenValid => DateTime.UtcNow < TokenExpiryTime;
}

public class ProductDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public int Stock { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    
    public List<OrderItemDto> Items { get; set; } = new();
    
    public decimal TotalAmount { get; set; }
    
    public string Status { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
}
