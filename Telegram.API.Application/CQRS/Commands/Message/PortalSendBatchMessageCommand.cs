using System;
using MediatR;
using Telegram.API.Domain.Entities.Fields;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Domain;

namespace Telegram.API.Application.CQRS.Commands.Message;

public sealed record PortalSendBatchMessageCommand : IRequest<PortalSendBatchMessageCommandResult>, IHasItems<BatchMessageItem>,
    IPortalBatchMessages
{
    public required int BotId { get; init; }
    public required string EncryptedCustomerId { get; init; }
    public required List<BatchMessageItem> Items { get; init; }
    public string? CampDescription { get; init; }
    public DateTime? ScheduledDatetime { get; init; }
    public SplitBulk? SplitBulk { get; init; }
    public bool? RemoveDuplicates { get; init; }
}

public sealed record PortalSendBatchMessageCommandResult(string ReferenceNumber);
