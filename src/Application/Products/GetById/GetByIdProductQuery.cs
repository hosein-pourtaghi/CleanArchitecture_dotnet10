
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Products.GetById;
 
public sealed record GetByIdProductQuery(Guid Id) : IQuery<ProductDto>;

