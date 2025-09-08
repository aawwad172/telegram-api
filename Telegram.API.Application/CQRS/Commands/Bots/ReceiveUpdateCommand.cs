using MediatR;
using Telegram.API.Domain.Entities.Bot;

namespace Telegram.API.Application.CQRS.Commands.Bots;

public record ReceiveUpdateCommand(string PublicId, string? SecretToken, TelegramUpdate Update) : IRequest<ReceiveUpdateCommandResult>;

public sealed record ReceiveUpdateCommandResult();