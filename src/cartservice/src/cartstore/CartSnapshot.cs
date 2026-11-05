using System.Collections.Generic;

namespace cartservice.cartstore;

public sealed class CartSnapshot
{
    public string UserId { get; set; } = string.Empty;

    public List<CartItemSnapshot> Items { get; set; } = [];
}

public sealed class CartItemSnapshot
{
    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
