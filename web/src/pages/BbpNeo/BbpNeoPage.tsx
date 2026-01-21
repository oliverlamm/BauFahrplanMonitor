import "./BbpNeoPage.css";
import "../../styles/importer-threads.css";

import { ImporterStatusBadge } from "../../components/importer/ImporterStatusBadge";
import { useBbpNeo } from "../../hooks/useBbpNeo";
import { mapImporterStatus } from "../../utils/importerStatusMapper";
import type { ImportJobState } from "../../models/importer-status";

/* ------------------------------------------------------------ */
function uiStateFromJob(
    state: ImportJobState
): "neutral" | "running" | "ok" | "warning" | "error" {
    switch (state) {
        case "Running":
            return "running";
        case "Finished":
            return "ok";
        case "FinishedWithErrors":
            return "warning";
        case "Cancelled":
        case "Failed":
            return "error";
        default:
            return "neutral";
    }
}

function progressPercent(done: number, total: number): number {
    if (!total || total <= 0) return 0;
    return Math.min(100, Math.round((done / total) * 100));
}

/* ============================================================ */
export default function BbpNeoPage() {
    const {
        status,
        files,
        selectedFile,
        setSelectedFile,
        loading,
        error,
        startImport,
        cancelImport,
        reloadFiles
    } = useBbpNeo();

    const badge = mapImporterStatus(status.state as ImportJobState);

    const percent = progressPercent(
        status.massnahmenFertig,
        status.massnahmenGesamt
    );

    const canStart =
        (status.state === "Idle" ||
            status.state === "Finished" ||
            status.state === "FinishedWithErrors" ||
            status.state === "Cancelled") &&
        !!selectedFile &&
        !loading;

    const canCancel = status.state === "Running";

    return (
        <div className="importer-page">
            <section className={`importer-card ${uiStateFromJob(status.state)}`}>

                {/* Header */}
                <header className="importer-header">
                    <div>
                        <h2>BBPNeo</h2>
                        <div className="importer-subtitle">
                            Import für Maßnahmen aus BBPNeo
                        </div>
                    </div>

                    <ImporterStatusBadge status={badge.cls} text={badge.text} />
                </header>

                {/* Controls */}
                <section className="bbp-controls">

                    <div className="bbp-row">
                        <div className="bbp-row-left">
                            <label>Datei</label>
                            <select
                                value={selectedFile ?? ""}
                                onChange={e =>
                                    setSelectedFile(
                                        e.target.value || null
                                    )
                                }
                                disabled={loading}
                            >
                                <option value="">
                                    — BBP-Datei auswählen —
                                </option>

                                {files.map(f => (
                                    <option key={f.fileName} value={f.fileName}>
                                        {f.fileName} —{" "}
                                        {(f.sizeBytes / 1024 / 1024).toFixed(0)} MB
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="bbp-row-right">
                            <button
                                className="btn"
                                onClick={reloadFiles}
                                disabled={loading}
                                title="Dateiliste neu laden"
                            >
                                <i className="fa-solid fa-rotate" />
                            </button>
                        </div>
                    </div>

                    <div className="bbp-row">
                        <div className="bbp-row-left" />

                        <div className="bbp-row-right">
                            <button
                                className="btn btn-primary"
                                disabled={!canStart}
                                onClick={startImport}
                            >
                                <i className="fa-solid fa-play" /> Start
                            </button>

                            <button
                                className="btn btn-secondary"
                                disabled={!canCancel}
                                onClick={cancelImport}
                            >
                                <i className="fa-solid fa-xmark" /> Stopp
                            </button>
                        </div>
                    </div>

                    {error && (
                        <div className="importer-error">
                            <i className="fa-solid fa-triangle-exclamation" />
                            &nbsp;{error}
                        </div>
                    )}
                </section>

                {/* Progress */}
                <section className="progress-block">
                    <div className="progress-label">
                        <span>Import-Fortschritt</span>
                        <span>{percent}%</span>
                    </div>

                    <div className="progress-bar">
                        <div
                            className="progress-value"
                            style={{ width: `${percent}%` }}
                        />
                    </div>

                    <div className="progress-subline">
                        {status.massnahmenFertig.toLocaleString()}
                        {" / "}
                        {status.massnahmenGesamt.toLocaleString()}
                        {" Maßnahmen"}

                        {status.errors > 0 && (
                            <> — ⚠ {status.errors} Fehler</>
                        )}
                    </div>
                </section>

                {/* Statistik */}
                <section className="bbpneo-stats">
                    <h4>Statistik</h4>

                    <div className="stat-row">
                        <span>Regelungen</span>
                        <span>{status.regelungen}</span>
                    </div>
                    <div className="stat-row">
                        <span>BvE</span>
                        <span>{status.bve}</span>
                    </div>
                    <div className="stat-row">
                        <span>APS</span>
                        <span>{status.aps}</span>
                    </div>
                    <div className="stat-row">
                        <span>IAV</span>
                        <span>{status.iav}</span>
                    </div>
                </section>

            </section>
        </div>
    );
}
