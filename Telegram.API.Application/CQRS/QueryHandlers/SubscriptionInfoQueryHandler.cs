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
            // Authenticate the Tenant using the provided username and password
            Customer customer = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

            if (customer is null)
                throw new UnauthorizedAccessException("Invalid username or password.");

            User? user = await _userRepository.GetUserAsync(request.PhoneNumber, request.BotKey) ?? throw new NotFoundException("User Not Subscribed!");

            SubscriptionInfoQueryResult result = user.Adapt<SubscriptionInfoQueryResult>();

            return result;
        }
        catch (UnauthorizedAccessException ex)
        {
            // Handle unauthorized access
            throw new UnauthorizedAccessException($"Authentication failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            throw new Exception($"An error occurred while processing the request: {ex.Message}");
        }
    }
}
