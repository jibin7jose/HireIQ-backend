using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Jobs.Queries.GenerateCoverLetter;

public sealed class GenerateCoverLetterQueryHandler : IRequestHandler<GenerateCoverLetterQuery, string>
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGeminiAiService _geminiAiService;

    public GenerateCoverLetterQueryHandler(
        IJobRepository jobRepository, 
        IUserRepository userRepository, 
        IGeminiAiService geminiAiService)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _geminiAiService = geminiAiService;
    }

    public async Task<string> Handle(GenerateCoverLetterQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Job), request.JobId);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var profile = user.UserProfile
            ?? throw new DomainException("User profile not found. Please complete your profile first.");

        var candidateProfile = $"Name: {profile.FullName}\nSkills: {string.Join(", ", profile.Skills)}\nExperience: {profile.ExperienceSummary}\nEducation: {profile.Education}";
        var jobDescription = $"Title: {job.Title}\nCompany: {job.Company?.Name}\nDescription: {job.Description}";

        return await _geminiAiService.GenerateCoverLetterAsync(candidateProfile, jobDescription, cancellationToken);
    }
}
