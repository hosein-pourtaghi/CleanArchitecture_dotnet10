using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace Application.Carts.GetById;

public sealed record GetCartByIdQuery(Guid Id) : IQuery<CartDto>;
