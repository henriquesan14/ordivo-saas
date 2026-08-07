using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Storage;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.ServiceOrders.ManageServiceOrder;

public sealed record UpdateServiceOrderCommand(Guid ServiceOrderId, Guid CustomerId, string Title, string Description,
    decimal Price, Guid? AssignedUserId, DateTimeOffset? ScheduledAt) : ICommand<ServiceOrderDto>;
public sealed record AddServiceOrderCommentCommand(Guid ServiceOrderId, string Text) : ICommand<ServiceOrderDto>;
public sealed record AddServiceOrderAttachmentCommand(Guid ServiceOrderId, string FileName, string ContentType, long Size, Stream Content) : ICommand<ServiceOrderDto>;
public sealed record DownloadServiceOrderAttachmentQuery(Guid ServiceOrderId, Guid AttachmentId) : IQuery<AttachmentDownloadDto>;
public sealed record DeleteServiceOrderAttachmentCommand(Guid ServiceOrderId, Guid AttachmentId) : ICommand<ServiceOrderDto>;
public sealed record AttachmentDownloadDto(Stream Content, string ContentType, string FileName, long Length);

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
        RuleFor(x => x.Size).GreaterThan(0).LessThanOrEqualTo(10 * 1024 * 1024);
        RuleFor(x => x.ContentType).Must(type => new[] { "application/pdf", "image/jpeg", "image/png", "image/webp", "text/plain" }.Contains(type.ToLowerInvariant())).WithMessage("Unsupported attachment type.");
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

public sealed class AddServiceOrderAttachmentCommandHandler(IServiceOrderRepository orders, IUnitOfWork unitOfWork, IUserContext user, IFileStorage storage, TimeProvider time) : ICommandHandler<AddServiceOrderAttachmentCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(AddServiceOrderAttachmentCommand command, CancellationToken ct)
    {
        var order = await orders.GetAsync(command.ServiceOrderId, ct);
        if (order is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Service order not found."));
        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        var storageKey = $"tenants/{user.TenantId:N}/service-orders/{order.Id:N}/{Guid.NewGuid():N}{extension}";
        await storage.UploadAsync(storageKey, command.Content, command.ContentType, ct);
        try
        {
            order.AddAttachment(user.UserId, user.Name ?? user.Email ?? "Unknown", Path.GetFileName(command.FileName), command.ContentType, command.Size, storageKey, time.GetUtcNow());
            await unitOfWork.SaveChangesAsync(ct); return Result.Success(order.ToDto());
        }
        catch { await storage.DeleteAsync(storageKey, ct); throw; }
    }
}

public sealed class DownloadServiceOrderAttachmentQueryHandler(IServiceOrderRepository orders, IFileStorage storage) : IQueryHandler<DownloadServiceOrderAttachmentQuery, AttachmentDownloadDto>
{
    public async Task<Result<AttachmentDownloadDto>> Handle(DownloadServiceOrderAttachmentQuery query, CancellationToken ct)
    {
        var order = await orders.GetAsync(query.ServiceOrderId, ct); var attachment = order?.GetAttachment(query.AttachmentId);
        if (attachment is null) return Result.Failure<AttachmentDownloadDto>(Error.NotFound("Attachment not found."));
        var file = await storage.DownloadAsync(attachment.StorageKey, ct);
        return file is null ? Result.Failure<AttachmentDownloadDto>(Error.NotFound("Attachment content not found.")) : Result.Success(new AttachmentDownloadDto(file.Content, file.ContentType, attachment.FileName, file.Length));
    }
}

public sealed class DeleteServiceOrderAttachmentCommandHandler(IServiceOrderRepository orders, IUnitOfWork unitOfWork, IFileStorage storage) : ICommandHandler<DeleteServiceOrderAttachmentCommand, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(DeleteServiceOrderAttachmentCommand command, CancellationToken ct)
    {
        var order = await orders.GetAsync(command.ServiceOrderId, ct); var attachment = order?.RemoveAttachment(command.AttachmentId);
        if (order is null || attachment is null) return Result.Failure<ServiceOrderDto>(Error.NotFound("Attachment not found."));
        await unitOfWork.SaveChangesAsync(ct); await storage.DeleteAsync(attachment.StorageKey, ct); return Result.Success(order.ToDto());
    }
}
