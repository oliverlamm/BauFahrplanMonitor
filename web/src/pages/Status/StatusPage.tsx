import "./StatusPage.css";

export default function StatusPage() {
    return (
        <>
            <div className="status-page-header">
                <h2>System-Status</h2>
                <p>BauFahrplanMonitor v0.1 • Session: Inverclair</p>
            </div>

            <div className="status-grid">
                {/* Datenbank */}
                <section className="status-card warning">
                    <div className="status-card-header">
                        <h3>Datenbankverbindung</h3>
                        <span className="badge warning">Warnung</span>
                    </div>

                    <div className="kv-list">
                        <div><span>Schema:</span><span>120, erwartet: 111</span></div>
                        <div><span>User:</span><span className="mono">infrago</span></div>
                        <div><span>Host:</span><span className="mono">192.168.1.24</span></div>
                        <div><span>Port:</span><span className="mono">5433</span></div>
                        <div><span>Database:</span><span className="mono">ujbaudb</span></div>
                        <div><span>Schema Version:</span><span className="mono">120 / 111</span></div>
                    </div>
                </section>

                {/* Allgemein */}
                <section className="status-card">
                    <div className="status-card-header">
                        <h3>Allgemein</h3>
                    </div>

                    <div className="kv-list">
                        <div><span>Import Threads:</span><span className="mono">6</span></div>
                        <div><span>Debugging:</span><span className="mono">false</span></div>
                        <div><span>StopAfterException:</span><span className="mono">true</span></div>
                        <div><span>EF Logging:</span><span className="mono">true</span></div>
                        <div><span>EF Sensitive:</span><span className="mono">true</span></div>
                        <div><span>Archivieren:</span><span className="mono">false</span></div>
                        <div><span>Nach Import löschen:</span><span className="mono">false</span></div>
                    </div>
                </section>

                {/* Importpfad */}
                <section className="status-card ok">
                    <div className="status-card-header">
                        <h3>Importpfad</h3>
                        <span className="badge ok">OK</span>
                    </div>

                    <div className="kv-list">
                        <div><span>Status:</span><span>Importpfad existiert</span></div>
                        <div className="mono">
                            <span>Pfad:</span>
                            <span>/home/oli/Dokumente/DBNetz/Import/</span>
                        </div>
                    </div>
                </section>

                {/* Archivpfad */}
                <section className="status-card ok">
                    <div className="status-card-header">
                        <h3>Archivpfad</h3>
                        <span className="badge ok">OK</span>
                    </div>

                    <div className="kv-list">
                        <div><span>Status:</span><span>Archivpfad existiert und ist beschreibbar</span></div>
                        <div className="mono">
                            <span>Pfad:</span>
                            <span>/home/oli/Dokumente/DBNetz/Archiv/</span>
                        </div>
                    </div>
                </section>
            </div>
        </>
    );
}
