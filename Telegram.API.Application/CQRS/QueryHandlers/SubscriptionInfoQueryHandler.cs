using Mapster;
using MediatR;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Entities.Bot;
using Telegram.API.Domain.Entities.User;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.QueryHandlers;

public class SubscriptionInfoQueryHandler(
    IAuthenticationService authenticationService,
    IRecipientRepository userRepository)
    : IRequestHandler<SubscriptionInfoQuery, SubscriptionInfoQueryResult>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IRecipientRepository _userRepository = userRepository;
    public async Task<SubscriptionInfoQueryResult> Handle(SubscriptionInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Authenticate the Customer using the provided username and password
            Customer? customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

            if (customer is null)
                throw new UnauthorizedException("Invalid username or password.");

            Bot? bot = await _authenticationService.ValidateBotIdAsync(request.BotId, customer.CustomerId);
            if (bot is null)
                throw new UnauthorizedException("Invalid Bot Key.");


            Recipient? user = await _userRepository.GetRecipientAsync(request.PhoneNumber, bot.BotId) ?? throw new NotFoundException("User Not Subscribed!");

            SubscriptionInfoQueryResult result = user.Adapt<SubscriptionInfoQueryResult>();

            return result;
        }
        catch (UnauthorizedException ex)
        {
            // Handle unauthorized access
            throw new UnauthorizedException($"Authentication failed: {ex.Message}");
        }
    }
}
