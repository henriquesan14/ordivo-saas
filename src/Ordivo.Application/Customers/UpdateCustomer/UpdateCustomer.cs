using FluentValidation;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string Document,
    string Phone,
    string? Email) : ICommand<CustomerDto>;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Document).NotEmpty().MaximumLength(20);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(20);
        RuleFor(command => command.Email).EmailAddress().MaximumLength(254)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));
    }
}

public sealed class UpdateCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCustomerCommand, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        var customer = await customers.GetAsync(command.CustomerId, ct);
        if (customer is null) return Result.Failure<CustomerDto>(Error.NotFound("Customer not found."));
        if (await customers.DocumentExistsAsync(command.Document.Trim(), ct, customer.Id))
            return Result.Failure<CustomerDto>(Error.Conflict("A customer with this document already exists."));

        customer.Update(command.Name, command.Document, command.Phone, command.Email);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(customer.ToDto());
    }
}
