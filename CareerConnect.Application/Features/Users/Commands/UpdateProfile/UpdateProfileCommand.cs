using System;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    bool ReceiveJobAlerts
) : IRequest;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.UserProfile == null)
            throw new DomainException("User profile not found.");

        user.UserProfile.ReceiveJobAlerts = request.ReceiveJobAlerts;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
