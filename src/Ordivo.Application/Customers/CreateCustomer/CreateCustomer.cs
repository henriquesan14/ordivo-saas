using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.Customers;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.Customers.CreateCustomer;
public sealed record CreateCustomerCommand(string Name, string Document, string Phone, string? Email) : ICommand<CustomerDto>;
public sealed class CreateCustomerCommandHandler(ICustomerRepository customers, IUnitOfWork unitOfWork, IUserContext userContext) : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        if (await customers.DocumentExistsAsync(command.Document.Trim(), ct)) return Result.Failure<CustomerDto>(Error.Conflict("A customer with this document already exists."));
        var customer = Customer.Create(userContext.TenantId, command.Name, command.Document, command.Phone, command.Email);
        await customers.AddAsync(customer, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(customer.ToDto());
    }
}
