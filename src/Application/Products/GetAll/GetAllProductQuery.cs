
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Products.GetAll;
 
public sealed record GetAllProductQuery() : IQuery<List<ProductDto>>;

