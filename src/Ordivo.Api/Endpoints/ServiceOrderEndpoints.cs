using Carter;
using Ordivo.Api.Common;
using Ordivo.Application.Common;
using Ordivo.Application.ServiceOrders;
using Ordivo.Application.ServiceOrders.ChangeServiceOrderStatus;
using Ordivo.Application.ServiceOrders.CreateServiceOrder;
using Ordivo.Application.ServiceOrders.GetServiceOrder;
using Ordivo.Application.ServiceOrders.ListServiceOrders;
using Ordivo.Application.ServiceOrders.ManageServiceOrder;
using Ordivo.Domain.ServiceOrders;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;
public sealed class ServiceOrderEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group=app.MapGroup("/api/service-orders").WithTags("Service orders").RequireAuthorization("TenantUser");
        group.MapPost("/",async(CreateServiceOrderCommand command,ICommandHandler<CreateServiceOrderCommand,ServiceOrderDto> handler,CancellationToken ct)=>{var r=await handler.Handle(command,ct);return r.IsSuccess?Results.Created($"/api/service-orders/{r.Value.Id}",r.Value):r.ToHttpResult();});
        group.MapGet("/{id:guid}",async(Guid id,IQueryHandler<GetServiceOrderQuery,ServiceOrderDto> handler,CancellationToken ct)=>(await handler.Handle(new(id),ct)).ToHttpResult());
        group.MapGet("/",async(string? search,ServiceOrderStatus? status,Guid? customerId,Guid? assignedUserId,DateTimeOffset? scheduledFrom,DateTimeOffset? scheduledTo,int? page,int? pageSize,string? sortBy,bool? descending,IQueryHandler<ListServiceOrdersQuery,PagedResult<ServiceOrderDto>> handler,CancellationToken ct)=>(await handler.Handle(new(search,status,customerId,assignedUserId,scheduledFrom,scheduledTo,page??1,pageSize??20,string.IsNullOrWhiteSpace(sortBy)?"createdAt":sortBy,descending??true),ct)).ToHttpResult());
        group.MapPut("/{id:guid}",async(Guid id,UpdateRequest r,ICommandHandler<UpdateServiceOrderCommand,ServiceOrderDto> h,CancellationToken ct)=>(await h.Handle(new(id,r.CustomerId,r.Title,r.Description,r.Price,r.AssignedUserId,r.ScheduledAt),ct)).ToHttpResult());
        group.MapPatch("/{id:guid}/status",async(Guid id,ChangeStatusRequest r,ICommandHandler<ChangeServiceOrderStatusCommand,ServiceOrderDto> h,CancellationToken ct)=>(await h.Handle(new(id,r.Status,r.Note),ct)).ToHttpResult());
        group.MapPost("/{id:guid}/comments",async(Guid id,CommentRequest r,ICommandHandler<AddServiceOrderCommentCommand,ServiceOrderDto> h,CancellationToken ct)=>(await h.Handle(new(id,r.Text),ct)).ToHttpResult());
        group.MapPost("/{id:guid}/attachments",async(Guid id,HttpRequest request,ICommandHandler<AddServiceOrderAttachmentCommand,ServiceOrderDto> h,CancellationToken ct)=>
        {
            if(!request.HasFormContentType)return Results.BadRequest(new{error="multipart/form-data is required."});
            var form=await request.ReadFormAsync(ct);var file=form.Files.GetFile("file");
            if(file is null)return Results.BadRequest(new{error="File is required."});
            await using var stream=file.OpenReadStream();return (await h.Handle(new(id,file.FileName,file.ContentType,file.Length,stream),ct)).ToHttpResult();
        });
        group.MapGet("/{id:guid}/attachments/{attachmentId:guid}/download",async(Guid id,Guid attachmentId,IQueryHandler<DownloadServiceOrderAttachmentQuery,AttachmentDownloadDto> h,CancellationToken ct)=>
        {var result=await h.Handle(new(id,attachmentId),ct);return result.IsSuccess?Results.Stream(result.Value.Content,result.Value.ContentType,result.Value.FileName,enableRangeProcessing:true):result.ToHttpResult();});
        group.MapDelete("/{id:guid}/attachments/{attachmentId:guid}",async(Guid id,Guid attachmentId,ICommandHandler<DeleteServiceOrderAttachmentCommand,ServiceOrderDto> h,CancellationToken ct)=>(await h.Handle(new(id,attachmentId),ct)).ToHttpResult());
    }
    private sealed record UpdateRequest(Guid CustomerId,string Title,string Description,decimal Price,Guid? AssignedUserId,DateTimeOffset? ScheduledAt);
    private sealed record ChangeStatusRequest(ServiceOrderStatus Status,string? Note);
    private sealed record CommentRequest(string Text);
}
