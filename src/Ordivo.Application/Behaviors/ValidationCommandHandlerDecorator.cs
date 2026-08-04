using FluentValidation;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Behaviors;

internal sealed class ValidationCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> innerHandler,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TCommand>(command);
        var results = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var errors = results.SelectMany(result => result.Errors).Where(error => error is not null).ToArray();

        if (errors.Length == 0)
            return await innerHandler.Handle(command, cancellationToken);

        var description = string.Join(" ", errors.Select(error => error.ErrorMessage).Distinct());
        return Result.Failure<TResponse>(Error.Validation(description));
    }
}
