export function mapWorkerState(state: string): { cls: string; text: string } {
    switch (state) {
        case "Idle":
            return { cls: "status-bereit", text: "Bereit" };

        case "Working":
            return { cls: "status-running", text: "Import läuft" };

        case "Stopping":
            return { cls: "status-warning", text: "Stoppt…" };

        case "Canceled":
            return { cls: "status-disabled", text: "Abgebrochen" };

        case "Error":
            return { cls: "status-error", text: "Fehler" };

        default:
            return { cls: "status-bereit", text: state };
    }
}
