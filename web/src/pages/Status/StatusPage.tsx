import "./StatusPage.css";
import {useStatus} from "../../hooks/useStatus";

export default function StatusPage() {
    const {data, loading, error} = useStatus();
    if (loading) {
        return <div>Lade Systemstatus…</div>;
    }

    if (error || !data) {
        return <div className="error">Fehler: {error}</div>;
    }
    return (
        <>
            <div className="status-page-header">
                <h2>System-Status</h2>
                <p>{data.name} </p>
            </div>

            <div className="status-grid">
                {/* Datenbank */}
                <section className={`status-card ${data.databaseStatus.status.toLowerCase()}`}>
                    <div className="status-card-header">
                        <h3>Datenbankverbindung</h3>
                        <span className={`badge ${data.databaseStatus.status.toLowerCase()}`}>
                            {data.databaseStatus.status}
                        </span>
                    </div>

                    <div className="kv-list">
                        <div><span>Schema:</span><span>{data.databaseStatus.message}</span></div>
                        <div><span>User:</span><span className="mono">{data.datenbank.user}</span></div>
                        <div><span>Host:</span><span className="mono">{data.datenbank.host}</span></div>
                        <div><span>Port:</span><span className="mono">{data.datenbank.port}</span></div>
                        <div><span>Database:</span><span className="mono">{data.datenbank.database}</span></div>
                        <div><span>Schema Version:</span><span
                            className="mono">{data.databaseStatus.currentSchemaVersion} / {data.databaseStatus.expectedSchemaVersion}</span>
                        </div>
                    </div>
                </section>

                {/* Allgemein */}
                <section className="status-card">
                    <div className="status-card-header">
                        <h3>Allgemein</h3>
                    </div>

                    <div className="kv-list">
                        <div><span>Import Threads:</span><span className="mono">{data.allgemein.importThreads}</span>
                        </div>
                        <div><span>Debugging:</span><span
                            className="mono">{data.allgemein.debugging ? "Ja" : "Nein"}</span></div>
                        <div><span>StopAfterException:</span><span
                            className="mono">{data.allgemein.stopAfterException ? "Ja" : "Nein"}</span></div>
                        <div><span>EF Logging:</span><span
                            className="mono">{data.datenbank.efLogging ? "Ja" : "Nein"}</span></div>
                        <div><span>EF Sensitive:</span><span
                            className="mono">{data.datenbank.efSensitiveLogging ? "Ja" : "Nein"}</span></div>
                        <div><span>Archivieren:</span><span
                            className="mono">{data.datei.archivieren ? "Ja" : "Nein"}</span></div>
                        <div><span>Nach Import löschen:</span><span
                            className="mono">{data.datei.nachImportLoeschen ? "Ja" : "Nein"}</span></div>
                    </div>
                </section>

                <section
                    className={`status-card ${data.paths.import.status.toLowerCase()}`}
                >
                    <div className="status-card-header">
                        <h3>Importpfad</h3>
                        <span
                            className={`badge ${data.paths.import.status.toLowerCase()}`}
                        >
                            {data.paths.import.status}
                        </span>
                    </div>

                    <div className="kv-list">
                        <div>
                            <span>Status:</span>
                            <span>{data.paths.import.message}</span>
                        </div>
                        <div className="mono">
                            <span>Pfad:</span>
                            <span>{data.datei.importpfad}</span>
                        </div>
                    </div>
                </section>


                {/* Archivpfad */}
                <section
                    className={`status-card ${data.paths.archive.status.toLowerCase()}`}
                >
                    <div className="status-card-header">
                        <h3>Archivpfad</h3>
                        <span
                            className={`badge ${data.paths.archive.status.toLowerCase()}`}
                        >
                            {data.paths.archive.status}
                        </span>
                    </div>

                    <div className="kv-list">
                        <div>
                            <span>Status:</span>
                            <span>{data.paths.archive.message}</span>
                        </div>
                        <div className="mono">
                            <span>Pfad:</span>
                            <span>{data.datei.archivpfad}</span>
                        </div>
                    </div>
                </section>

            </div>
        </>
    );
}
