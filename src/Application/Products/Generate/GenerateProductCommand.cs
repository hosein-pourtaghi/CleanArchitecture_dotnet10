
using Application.Abstractions.Messaging;

namespace Application.Products.Generate;

public sealed record GenerateProductCommand() : ICommand<bool>;
