using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Carts.Get;

public sealed record GetAllCartQuery() : IQuery<List<CartDto>>;
