using Microsoft.AspNetCore.Mvc;
using Mignow.Atc.Api.Contracts;
using Mignow.Atc.Application.Abstractions;
using Mignow.Atc.Application.Common;
using Mignow.Atc.Application.Dtos.Registry;
using Mignow.Atc.Application.Features.Customers.Create;
using Mignow.Atc.Application.Features.Customers.Delete;
using Mignow.Atc.Application.Features.Customers.List;
using Mignow.Atc.Application.Features.Customers.Update;

namespace Mignow.Atc.Api.Controllers;

[Route("v1/customers")]
[Tags("Customers")]
public sealed class CustomersController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult<CustomerResponse>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerResponse>>>> List(
        [FromServices] IQueryHandler<ListCustomersQuery, Result<PagedResult<CustomerResponse>>> handler,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] bool? active = null,
        CancellationToken ct = default) =>
        OkOrFailure(await handler.Handle(new ListCustomersQuery(page, pageSize, search, active), ct));

    [HttpPost]
    [ProducesResponseType<ApiResponse<CustomerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Create(CreateCustomerCommand request,
        [FromServices] ICommandHandler<CreateCustomerCommand, Result<CustomerResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(request, ct));

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<CustomerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Update(Guid id, UpdateCustomerRequest request,
        [FromServices] ICommandHandler<UpdateCustomerCommand, Result<CustomerResponse>> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new UpdateCustomerCommand(id, request.Name, request.Code, request.Active), ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(Guid id, [FromServices] ICommandHandler<DeleteCustomerCommand, Result> handler, CancellationToken ct) =>
        OkOrFailure(await handler.Handle(new DeleteCustomerCommand(id), ct));
}
