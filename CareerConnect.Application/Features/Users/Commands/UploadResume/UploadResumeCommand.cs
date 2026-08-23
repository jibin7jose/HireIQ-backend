using MediatR;

namespace CareerConnect.Application.Features.Users.Commands.UploadResume;

public record UploadResumeCommand(
    Guid UserId,
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<string>;
