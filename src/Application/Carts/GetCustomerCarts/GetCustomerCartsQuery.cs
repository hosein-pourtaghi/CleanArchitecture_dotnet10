using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Carts.GetCustomerCarts;

public sealed record GetCustomerCartsQuery(Guid customerId , int page = 1,int pageSize = 10) : IQuery<List<CartDto>>;
