namespace EDMS.Core.Domain;

public enum EncounterState
{
    Waiting,
    InService,
    Completed,
    Discharged,
    Admitted,
    Cancelled
}

public enum TriageCategory
{
    Routine,
    Diagnostics,
    MinorProcedure,
    TherapySession,
    UrgentCare
}

public enum FinalDisposition
{
    Outpatient,
    Inpatient,
    Referred,
    LAMA,
    Deceased,
    LeftWithoutSeen
}

public enum StaffRole
{
    Admin,
    Doctor,
    Nurse,
    Staff
}
