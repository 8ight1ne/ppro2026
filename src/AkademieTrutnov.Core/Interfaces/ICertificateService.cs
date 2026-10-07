using AkademieTrutnov.Core.Entities;
using AkademieTrutnov.Core.Enums;

namespace AkademieTrutnov.Core.Interfaces;

public interface ICertificateService
{
    Task<Osvedceni> IssueCertificateAsync(
        Guid prihlaskaId, 
        VysledekHodnoceni vysledek, 
        string vystavil, 
        CancellationToken ct = default);

    Task<Osvedceni?> GetCertificateByNumberAsync(
        string cisloOsvedceni, 
        CancellationToken ct = default);

    Task<List<Osvedceni>> GetCertificatesForTerminAsync(
        Guid terminId, 
        CancellationToken ct = default);
}
