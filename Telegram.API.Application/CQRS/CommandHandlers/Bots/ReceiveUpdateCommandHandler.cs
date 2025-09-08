using System.Xml.XPath;
using MediatR;
using Telegram.API.Application.CQRS.Commands.Bots;
using Telegram.API.Application.HelperServices;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Entities.Telegram;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Clients;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.CommandHandlers.Bots;

public class ReceiveUpdateCommandHandler(
    IBotRepository botRepository,
    ITelegramUserChatsRepository userChatsRepository,
    ITelegramClient telegramClient,
    IAuthenticationService authenticationService)
    : IRequestHandler<ReceiveUpdateCommand, ReceiveUpdateCommandResult>
{
    private readonly IBotRepository _botRepository = botRepository;
    private readonly ITelegramUserChatsRepository _chatsRepository = userChatsRepository;
    private readonly ITelegramClient _telegramClient = telegramClient;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    public async Task<ReceiveUpdateCommandResult> Handle(ReceiveUpdateCommand request, CancellationToken cancellationToken)
    {
        // 1) Resolve bot by PublicId
        Bot? bot = await _botRepository.GetByPublicIdAsync(request.PublicId, cancellationToken)
                  ?? throw new NotFoundException("Bot not found for publicId");

        // 2) Validate secret if configured
        if (!WebhookHelpers.IsAuthorized(request, bot))
            throw new UnauthorizedException("Invalid Telegram webhook secret.");

        // 3) We only care about private /start messages
        TelegramUpdateMessage? msg = request.Update.Message;
        if (msg is null || msg.Chat is null)
            return new();

        bool isPrivate = string.Equals(msg.Chat.Type, "private", StringComparison.OrdinalIgnoreCase);
        if (!isPrivate) return new();

        string? text = msg.Text?.Trim();
        bool isStart = text?.StartsWith("/start", StringComparison.OrdinalIgnoreCase) == true;

        string chatId = msg.Chat.Id.ToString();
        long tgUserId = msg.From?.Id ?? 0;
        if (tgUserId == 0) return new();

        // 1) CONTACT MESSAGE → save phone, confirm, return
        if (msg.Contact is not null)
        {
            // (optional) ensure the shared contact belongs to the sender
            if (msg.Contact.UserId is long contactUserId && contactUserId != 0 && contactUserId != tgUserId)
                return new(); // ignore contacts of other users

            string? phone = msg.Contact.PhoneNumber?.Trim();
            if (CommandSanitizerHelpers.TryNormalizePhoneNumber(phone!, out string? normalized))
                phone = normalized;

            await _chatsRepository.AddAsync(
                botId: bot.BotId,
                chatId: chatId,
                phoneNumber: phone,
                telegramUserId: tgUserId,
                username: msg.From?.Username?.Trim(),
                firstName: msg.From?.FirstName?.Trim(),
                isActive: true,
                ct: cancellationToken
            );


            string token = _authenticationService.Decrypt(bot.EncryptedBotKey, cancellationToken);
            await _telegramClient.SendTextAsync(
                token,
                chatId,
                "Thanks! Your phone number was received and your subscription is complete.",
                cancellationToken);

            return new();
        }

        // 2) /start → create/update row, prompt for phone
        if (isStart)
        {
            await _chatsRepository.AddAsync(
                botId: bot.BotId,
                chatId: chatId,
                phoneNumber: null,                 // none yet
                telegramUserId: tgUserId,
                username: msg.From?.Username?.Trim(),
                firstName: msg.From?.FirstName?.Trim(),
                isActive: true,
                ct: cancellationToken
            );


            string token = _authenticationService.Decrypt(bot.EncryptedBotKey, cancellationToken);
            await _telegramClient.SendTextWithContactButtonAsync(
                botToken: token,
                    chatId: chatId,
                    text: "Please share your phone number to complete your subscription.",
                    buttonText: "Share phone number",
                    ct: cancellationToken
                );


            return new();
        }

        // ignore other messages
        return new();
    }
}
