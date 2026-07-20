//using Application.Common.DTOs;
//using Application.Common.DTOs.Shared;
//using Application.Common.Messaging;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace WebApi.Controllers;

//[Area("Product")]
//[Route("api/[controller]")]
//[Authorize] 
//[ResponseCache(Duration = 0, NoStore = true, VaryByHeader = "*")]
//public sealed class ProductsController :
//    CrudController<CreateProductCommand, GetAllProductQuery, ProductDto>,
//    IHaveGetById
//{
//    public ProductsController(IMediator mediator) : base(mediator) { }

//    protected override IQuery BuildGetAllQuery(PaginatedRequest filter)
//        => new GetAllProductQuery(filter);

//    protected override IQuery BuildGetByIdQuery(Guid id)
//        => new GetByIdProductQuery(id);

//    protected override ICommand BuildCreateCommand(CreateProductCommand command) => command;

//    protected override ICommand BuildUpdateCommand(Guid id, CreateProductCommand command)
//        => new UpdateProductCommand(id, command.Name, command.Price);

//    protected override ICommand BuildDeleteCommand(Guid id)
//        => new DeleteProductCommand(id);
//}
