namespace StarterKit.Api.Features.Users;

public sealed record MeResponse(Guid Id, string Email, DateTime CreatedUtc);
