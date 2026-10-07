using AkademieTrutnov.Core.Enums;

namespace AkademieTrutnov.Core.DTOs;

public record RegistrationResultDto(
    Guid PrihlaskaId,
    StavPrihlasky Stav,
    int? PoradiVPoradniku,
    DateTime? NabidkaExpirace,
    string Zprava
);

public record DochazkaRecordDto(
    Guid StudentId,
    bool Pritomen,
    string? Poznamka
);

public record StudentAttendanceSummaryDto(
    Guid StudentId,
    string CeleJmeno,
    int CelkemHodin,
    int OdchozenoHodin,
    double ProcentoDochazky,
    bool SplnujePodminkuProAkreditaci // >= 70%
);

public record TerminUtilizationDto(
    Guid TerminId,
    string KurzNazev,
    string UcebnaNazev,
    DateTime DatumOd,
    DateTime DatumDo,
    int Kapacita,
    int PocetPrihlasenych,
    int PocetVNabidce,
    int PocetVPoradniku,
    double ProcentoZaplneni,
    StavTerminu Stav
);

public record LecturerHoursDto(
    Guid LektorId,
    string JmenoPrijmeni,
    string Email,
    bool Externista,
    int PocetTerminu,
    int CelkemOducenychHodin
);

public record MultiCourseStudentDto(
    Guid StudentId,
    string JmenoPrijmeni,
    string Email,
    int PocetKurzuVRoce,
    List<string> SeznamKurzu
);
