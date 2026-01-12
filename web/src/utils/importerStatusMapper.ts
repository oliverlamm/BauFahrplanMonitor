import type { ImporterStatus } from "../api/zvfExportApi";

export function mapImporterStatus(
    state: ImporterStatus
): { cls: string; text: string } {

    switch (state) {

        case "Idle":
            return { cls: "status-bereit", text: "Bereit" };

        case "Starting":
            return { cls: "status-scan", text: "Starte" };

        case "Scanning":
            return { cls: "status-scan", text: "Scannen" };

        case "Scanned":
            return { cls: "status-success", text: "Scan abgeschlossen" };

        case "Running":
            return { cls: "status-running", text: "Importiere" };

        case "Stopping":
            return { cls: "status-warning", text: "Stoppt" };

        case "Finished":
            return { cls: "status-success", text: "Abgeschlossen" };

        case "FinishedWithErrors":
            return { cls: "status-warning", text: "Abgeschlossen (Fehler)" };

        case "Aborted":
            return { cls: "status-disabled", text: "Abgebrochen" };

        case "Failed":
            return { cls: "status-error", text: "Fehler" };

        default:
            return { cls: "status-bereit", text: state };
    }
}

