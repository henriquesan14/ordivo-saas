using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.ServiceOrders;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.ServiceOrders.CreateServiceOrder;
public sealed record CreateServiceOrderCommand(Guid CustomerId, string Title, string Description, decimal Price, Guid? AssignedUserId, DateTimeOffset? ScheduledAt) : ICommand<ServiceOrderDto>;
public sealed class CreateServiceOrderCommandHandler(IServiceOrderRepository orders, ICustomerRepository customers, IUserRepository users, IUnitOfWork unitOfWork, IUserContext userContext, TimeProvider timeProvider) : ICommandHandler<CreateServiceOrderCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(CreateServiceOrderCommand command, CancellationToken ct)
    {
        if (await customers.GetAsync(command.CustomerId, ct) is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Customer not found."));
        if (command.AssignedUserId.HasValue && await users.GetByIdAsync(command.AssignedUserId.Value, ct) is null)
            return Result.Failure<ServiceOrderDto>(Error.NotFound("Assigned user not found."));
        var order = ServiceOrder.Create(userContext.TenantId, await orders.NextSequenceAsync(ct), command.CustomerId,
            command.Title, command.Description, command.Price, command.AssignedUserId, command.ScheduledAt,
            userContext.Name ?? userContext.Email ?? "Unknown", timeProvider.GetUtcNow());
        await orders.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(order.ToDto());
    }
}
