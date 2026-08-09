using System;
using System.Threading;
using System.Threading.Tasks;
using CareerConnect.Application.DTOs.Messages;
using CareerConnect.Application.Interfaces;
using CareerConnect.Domain.Entities;
using CareerConnect.Domain.Exceptions;
using MediatR;

namespace CareerConnect.Application.Features.Messages.Commands.SendMessage;

public record SendMessageCommand(
    Guid ApplicationId,
    Guid SenderUserId,
    string Content
) : IRequest<MessageDto>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ICompanyRepository _companyRepository;

    public SendMessageCommandHandler(
        IApplicationRepository applicationRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ICompanyRepository companyRepository)
    {
        _applicationRepository = applicationRepository;
        _messageRepository     = messageRepository;
        _userRepository        = userRepository;
        _unitOfWork            = unitOfWork;
        _notificationService   = notificationService;
        _companyRepository     = companyRepository;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application", request.ApplicationId);

        var sender = await _userRepository.GetByIdAsync(request.SenderUserId, cancellationToken)
            ?? throw new NotFoundException("User", request.SenderUserId);

        Guid receiverId;
        string senderName;

        if (sender.Role == Domain.Enums.UserRole.Employer)
        {
            // Employer sending to Candidate
            if (application.UserProfile == null) throw new DomainException("Candidate profile not found.");
            receiverId = application.UserProfile.UserId;
            
            var company = await _companyRepository.GetByAdminUserIdAsync(sender.Id, cancellationToken);
            senderName = company?.Name ?? "Employer";
        }
        else
        {
            // Candidate sending to Employer
            if (application.Job?.Company == null) throw new DomainException("Employer not found.");
            receiverId = application.Job.Company.AdminUserId;
            senderName = application.UserProfile?.FullName ?? "Candidate";
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ApplicationId = request.ApplicationId,
            SenderUserId = request.SenderUserId,
            ReceiverUserId = receiverId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto(
            message.Id,
            message.ApplicationId,
            message.SenderUserId,
            senderName,
            message.ReceiverUserId,
            message.Content,
            message.SentAt,
            message.IsRead
        );

        // Broadcast to receiver via SignalR
        await _notificationService.SendChatMessageAsync(receiverId.ToString(), dto);
        await _notificationService.SendNotificationAsync(receiverId.ToString(), $"New message from {senderName}", "Info");

        return dto;
    }
}
