using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.ServiceOrders.ManageServiceOrder;

public sealed record UpdateServiceOrderCommand(Guid ServiceOrderId, Guid CustomerId, string Title, string Description,
    decimal Price, Guid? AssignedUserId, DateTimeOffset? ScheduledAt) : ICommand<ServiceOrderDto>;
public sealed record AddServiceOrderCommentCommand(Guid ServiceOrderId, string Text) : ICommand<ServiceOrderDto>;
public sealed record AddServiceOrderAttachmentCommand(Guid ServiceOrderId, string FileName, string ContentType, long Size, string StorageKey) : ICommand<ServiceOrderDto>;

public sealed class UpdateServiceOrderCommandValidator : AbstractValidator<UpdateServiceOrderCommand>
{
    public UpdateServiceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty(); RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).MaximumLength(4000); RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
public sealed class AddServiceOrderCommentCommandValidator : AbstractValidator<AddServiceOrderCommentCommand>
{
    public AddServiceOrderCommentCommandValidator() => RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
}
public sealed class AddServiceOrderAttachmentCommandValidator : AbstractValidator<AddServiceOrderAttachmentCommand>
{
    public AddServiceOrderAttachmentCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255); RuleFor(x => x.ContentType).NotEmpty().MaximumLength(150);
        RuleFor(x => x.StorageKey).NotEmpty().MaximumLength(1000); RuleFor(x => x.Size).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateServiceOrderCommandHandler(IServiceOrderRepository orders, ICustomerRepository customers, IUserRepository users, IUnitOfWork unitOfWork) : ICommandHandler<UpdateServiceOrderCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(UpdateServiceOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetAsync(command.ServiceOrderId, ct);
        if (order is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Service order not found."));
        if (await customers.GetAsync(command.CustomerId, ct) is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Customer not found."));
        if (command.AssignedUserId.HasValue && await users.GetByIdAsync(command.AssignedUserId.Value, ct) is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Assigned user not found."));
        order.Update(command.CustomerId, command.Title, command.Description, command.Price, command.AssignedUserId, command.ScheduledAt);
        await unitOfWork.SaveChangesAsync(ct); return Result.Success(order.ToDto());
    }
}

public sealed class AddServiceOrderCommentCommandHandler(IServiceOrderRepository orders, IUnitOfWork unitOfWork, IUserContext user, TimeProvider time) : ICommandHandler<AddServiceOrderCommentCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(AddServiceOrderCommentCommand command, CancellationToken ct)
    {
        var order = await orders.GetAsync(command.ServiceOrderId, ct);
        if (order is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Service order not found."));
        order.AddComment(user.UserId, user.Name ?? user.Email ?? "Unknown", command.Text, time.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct); return Result.Success(order.ToDto());
    }
}

public sealed class AddServiceOrderAttachmentCommandHandler(IServiceOrderRepository orders, IUnitOfWork unitOfWork, IUserContext user, TimeProvider time) : ICommandHandler<AddServiceOrderAttachmentCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(AddServiceOrderAttachmentCommand command, CancellationToken ct)
    {
        var order = await orders.GetAsync(command.ServiceOrderId, ct);
        if (order is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Service order not found."));
        order.AddAttachment(user.UserId, user.Name ?? user.Email ?? "Unknown", command.FileName, command.ContentType, command.Size, command.StorageKey, time.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct); return Result.Success(order.ToDto());
    }
}
