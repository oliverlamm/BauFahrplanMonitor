export type ImportJobState =
    | "Idle"
    | "Starting"
    | "Scanning"
    | "Scanned"
    | "Running"
    | "Stopping"
    | "Finished"
    | "FinishedWithErrors"
    | "Cancelled"
    | "Failed";
