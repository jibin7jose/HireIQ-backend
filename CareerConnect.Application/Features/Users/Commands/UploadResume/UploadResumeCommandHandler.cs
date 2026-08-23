using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using CareerConnect.Domain.Entities;
using MediatR;

namespace CareerConnect.Application.Features.Users.Commands.UploadResume;

public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IGeminiAiService _geminiAiService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadResumeCommandHandler(
        IUserRepository userRepository,
        IStorageService storageService,
        IGeminiAiService geminiAiService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _geminiAiService = geminiAiService;
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

        // Try to parse the PDF and call Gemini
        try 
        {
            // Reset stream position for PDF parsing
            request.FileStream.Position = 0;
            
            using var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(request.FileStream);
            var textBuilder = new System.Text.StringBuilder();
            
            foreach (var page in pdfDocument.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }
            
            var resumeText = textBuilder.ToString();
            
            if (!string.IsNullOrWhiteSpace(resumeText))
            {
                var geminiJson = await _geminiAiService.ParseResumeAsync(resumeText, cancellationToken);
                
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(geminiJson);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("Skills", out var skillsProp) && skillsProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    profile.Skills = skillsProp.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrEmpty(x)).ToList();
                }
                
                if (root.TryGetProperty("ExperienceSummary", out var expProp))
                {
                    profile.ExperienceSummary = expProp.GetString() ?? "";
                }
                
                if (root.TryGetProperty("Education", out var eduProp))
                {
                    profile.Education = eduProp.GetString() ?? "";
                }
            }
        }
        catch (Exception ex)
        {
            // Log error, but don't fail the upload if parsing fails
            Console.WriteLine($"Failed to parse resume with AI: {ex.Message}");
        }
        
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return publicUrl;
    }
}
