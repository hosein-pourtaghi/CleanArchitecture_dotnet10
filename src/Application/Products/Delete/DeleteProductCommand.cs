
using Application.Abstractions.Messaging;
using Application.Common.DTOs;

namespace  Application.Products.Delete;
 
public sealed record DeleteProductCommand(Guid Id) : ICommand;
