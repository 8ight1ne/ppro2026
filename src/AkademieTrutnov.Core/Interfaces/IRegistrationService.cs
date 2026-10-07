using AkademieTrutnov.Core.DTOs;
using AkademieTrutnov.Core.Entities;

namespace AkademieTrutnov.Core.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResultDto> RegisterStudentAsync(
        Guid studentId, 
        Guid terminId, 
        bool isLateAdminEnrollment = false, 
        string? adminNote = null, 
        CancellationToken ct = default);

    Task<Prihlaska> CancelRegistrationAsync(
        Guid prihlaskaId, 
        string duvod, 
        CancellationToken ct = default);

    Task<Prihlaska> AcceptOfferAsync(
        Guid prihlaskaId, 
        CancellationToken ct = default);

    Task<Prihlaska> DeclineOfferAsync(
        Guid prihlaskaId, 
        CancellationToken ct = default);

    Task<int> ProcessExpiredOffersAsync(
        CancellationToken ct = default);
}
