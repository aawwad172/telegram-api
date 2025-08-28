using Mapster;
using MediatR;
using Telegram.API.Application.CQRS.Queries;
using Telegram.API.Domain.Entities;
using Telegram.API.Domain.Exceptions;
using Telegram.API.Domain.Interfaces.Application;
using Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

namespace Telegram.API.Application.CQRS.QueryHandlers;

public class SubscriptionInfoQueryHandler(
    IAuthenticationService authenticationService,
    IUserRepository userRepository) : IRequestHandler<SubscriptionInfoQuery, SubscriptionInfoQueryResult>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IUserRepository _userRepository = userRepository;
    public async Task<SubscriptionInfoQueryResult> Handle(SubscriptionInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Authenticate the Customer using the provided username and password
            Customer? customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

            if (customer is null)
                throw new UnauthorizedException("Invalid username or password.");

            Bot? bot = await _authenticationService.ValidateBotKeyAsync(request.BotKey, customer.CustomerId);
            if (bot is null)
                throw new UnauthorizedException("Invalid Bot Key.");


            User? user = await _userRepository.GetUserAsync(request.PhoneNumber, bot.BotId) ?? throw new NotFoundException("User Not Subscribed!");

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
