using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.DTOs.Messages;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Messages.Queries.GetMessages;

public record GetMessagesQuery(
    Guid ApplicationId,
    Guid RequestingUserId
) : IRequest<IEnumerable<MessageDto>>;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, IEnumerable<MessageDto>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;

    public GetMessagesQueryHandler(
        IApplicationRepository applicationRepository,
        IMessageRepository messageRepository,
        ICompanyRepository companyRepository,
        IUserRepository userRepository)
    {
        _applicationRepository = applicationRepository;
        _messageRepository     = messageRepository;
        _companyRepository     = companyRepository;
        _userRepository        = userRepository;
    }

    public async Task<IEnumerable<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application", request.ApplicationId);

        var requester = await _userRepository.GetByIdAsync(request.RequestingUserId, cancellationToken)
            ?? throw new UnauthorizedException("User not found.");

        // Authorization check
        if (requester.Role == Domain.Enums.UserRole.Employer)
        {
            var company = await _companyRepository.GetByAdminUserIdAsync(requester.Id, cancellationToken)
                ?? throw new UnauthorizedException("Company not found.");
            
            if (application.Job?.CompanyId != company.Id)
                throw new UnauthorizedException("You can only view messages for applications to your own jobs.");
        }
        else if (requester.Role == Domain.Enums.UserRole.Candidate)
        {
            if (application.UserProfile?.UserId != requester.Id)
                throw new UnauthorizedException("You can only view messages for your own applications.");
        }
        else
        {
            throw new UnauthorizedException("Admins cannot view application chats.");
        }

        var messages = await _messageRepository.GetMessagesByApplicationIdAsync(request.ApplicationId, cancellationToken);

        // Pre-fetch sender names for a simple mapping
        var companyCache = application.Job?.Company;
        
        return messages.Select(m => {
            string senderName = "User";
            if (m.Sender?.Role == Domain.Enums.UserRole.Employer)
                senderName = companyCache?.Name ?? "Employer";
            else
                senderName = application.UserProfile?.FullName ?? "Candidate";
                
            return new MessageDto(
                m.Id,
                m.ApplicationId,
                m.SenderUserId,
                senderName,
                m.ReceiverUserId,
                m.Content,
                m.SentAt,
                m.IsRead
            );
        });
    }
}
