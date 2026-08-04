using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.ServiceOrders;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.ServiceOrders.ChangeServiceOrderStatus;
public sealed record ChangeServiceOrderStatusCommand(Guid ServiceOrderId, ServiceOrderStatus Status) : ICommand<ServiceOrderDto>;
public sealed class ChangeServiceOrderStatusCommandHandler(IServiceOrderRepository orders, IUnitOfWork unitOfWork) : ICommandHandler<ChangeServiceOrderStatusCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(ChangeServiceOrderStatusCommand command, CancellationToken ct)
    {
        var order = await orders.GetAsync(command.ServiceOrderId, ct);
        if (order is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Service order not found."));
        try
        {
            order.ChangeStatus(command.Status);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(order.ToDto());
        }
        catch (InvalidOperationException ex) { return Result.Failure<ServiceOrderDto>(Error.Conflict(ex.Message)); }
    }
}
