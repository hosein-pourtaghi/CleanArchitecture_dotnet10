
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Products.Update;

public sealed record UpdateProductCommand(
        Guid Id
    ) : ICommand; 

