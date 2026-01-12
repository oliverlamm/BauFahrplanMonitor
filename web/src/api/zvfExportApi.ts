// =========================================================
// ZvF Export API Client
// =========================================================

// ⚠️ TYPE-EXPORT (nur Typ, kein Runtime-Code)
export type ImporterStatus =
    | "Idle"
    | "Starting"
    | "Scanning"
    | "Scanned"
    | "Running"
    | "Stopping"
    | "Finished"
    | "FinishedWithErrors"
    | "Aborted"
    | "Failed";


export type WorkerState =
    | "Idle"
    | "Working"
    | "Stopping"
    | "Canceled"
    | "Error";

export interface ImportWorkerStatus {
    workerId: number;
    state: WorkerState;
    currentFile: string | null;
    startedAt: string | null;
    processedItems: number;
    errors: number;
    progressMessage: string;
}

export interface ScanStat {
    zvF_New: number;
    zvF_Imported: number;

    ueB_New: number;
    ueB_Imported: number;

    fplo_New: number;
    fplo_Imported: number;

    kss_New: number;
    kss_Imported: number;
}


// -----------------------------
// POST /import
// -----------------------------
export async function startZvFExportImport(): Promise<void> {
    const res = await fetch("/api/import/zvfexport/import", {
        method: "POST"
    });

    if (!res.ok) {
        throw new Error("Import konnte nicht gestartet werden");
    }
}

// -----------------------------
// POST /cancel
// -----------------------------
export async function cancelZvFExportImport(): Promise<void> {
    const res = await fetch("/api/import/zvfexport/cancel", {
        method: "POST"
    });

    if (!res.ok) {
        throw new Error("Import konnte nicht abgebrochen werden");
    }
}

// -----------------------------
// POST /scan
// -----------------------------
export type ZvFFileFilter = number;

export async function scanZvFExport(
    filter: ZvFFileFilter
): Promise<void> {

    const res = await fetch("/api/import/zvfexport/scan", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ filter })
    });

    if (!res.ok) {
        throw new Error("Scan konnte nicht gestartet werden");
    }
}

// -----------------------------
// GET /status
// -----------------------------
export interface ZvFExportStatus {
    state: ImporterStatus;

    // 🔍 Scan
    scanTotalFiles: number;
    scanProcessedFiles: number;

    // 🚚 Import
    importTotalItems: number;
    importProcessedItems: number;
    importErrorItems: number;

    // Threads
    workers: ImportWorkerStatus[];
    activeWorkers: number;

    currentFile: string | null;
    startedAt: string | null;

    scanStat: ScanStat;
}

export async function getZvFExportStatus(): Promise<ZvFExportStatus> {
    const res = await fetch("/api/import/zvfexport/status");
    if (!res.ok) throw new Error("Status konnte nicht geladen werden");
    return res.json();
}

