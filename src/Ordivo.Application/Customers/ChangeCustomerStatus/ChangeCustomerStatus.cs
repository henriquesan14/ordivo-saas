using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Customers.ChangeCustomerStatus;

public sealed record ChangeCustomerStatusCommand(Guid CustomerId, bool IsActive) : ICommand<CustomerDto>;

public sealed class ChangeCustomerStatusCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeCustomerStatusCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(ChangeCustomerStatusCommand command, CancellationToken ct)
    {
        var customer = await customers.GetAsync(command.CustomerId, ct);
        if (customer is null) return Result.Failure<CustomerDto>(Error.NotFound("Customer not found."));

        if (command.IsActive) customer.Activate(); else customer.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(customer.ToDto());
    }
}
