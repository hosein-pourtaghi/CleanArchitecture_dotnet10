
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Products.Create;

public sealed record CreateProductCommand(
    ) : ICommand<Guid>; 
