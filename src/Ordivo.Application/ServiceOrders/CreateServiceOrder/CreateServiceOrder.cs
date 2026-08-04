using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.ServiceOrders;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.ServiceOrders.CreateServiceOrder;
public sealed record CreateServiceOrderCommand(Guid CustomerId, string Title, string Description, decimal Price) : ICommand<ServiceOrderDto>;
public sealed class CreateServiceOrderCommandHandler(IServiceOrderRepository orders, ICustomerRepository customers, IUnitOfWork unitOfWork, IUserContext userContext) : ICommandHandler<CreateServiceOrderCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(CreateServiceOrderCommand command, CancellationToken ct)
    {
        if (await customers.GetAsync(command.CustomerId, ct) is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Customer not found."));
        var order = ServiceOrder.Create(userContext.TenantId, command.CustomerId, command.Title, command.Description, command.Price);
        await orders.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(order.ToDto());
    }
}
