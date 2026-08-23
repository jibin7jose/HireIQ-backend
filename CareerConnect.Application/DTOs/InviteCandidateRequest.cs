using System;

namespace CareerConnect.Application.DTOs;

public class InviteCandidateRequest
{
    public Guid CandidateUserId { get; set; }
    public int AiMatchScore { get; set; }
}
