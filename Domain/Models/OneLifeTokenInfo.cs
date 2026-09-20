namespace LogMate.Domain.Models;

public record OneLifeTokenInfo(string LogId, string? UserId, int? Mode, DateTime Ttl);
