using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using CareerConnect.Domain.Entities;
using MediatR;

namespace CareerConnect.Application.Features.Users.Commands.UploadResume;

public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadResumeCommandHandler(
        IUserRepository userRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(UploadResumeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var profile = user.UserProfile
            ?? throw new NotFoundException("User profile not found. Please complete your profile first.");

        // If the user already has a resume, delete the old one from storage (optional, but good practice)
        if (!string.IsNullOrEmpty(profile.ResumeUrl))
        {
            try
            {
                await _storageService.DeleteFileAsync(profile.ResumeUrl, cancellationToken);
            }
            catch
            {
                // Ignore delete errors to ensure new upload succeeds
            }
        }

        var publicUrl = await _storageService.UploadFileAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);

        profile.ResumeUrl = publicUrl;
        
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return publicUrl;
    }
}
