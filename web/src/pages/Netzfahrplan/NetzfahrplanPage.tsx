import "./NetzfahrplanPage.css";
import "../../styles/importer-threads.css";

export default function NetzfahrplanPage() {
    return (
        <div className="importer-page">

            <section className="importer-card">
                {/* Header */}
                <section className="importer-header">

                    <div className="importer-header-left">
                        <h2>Netzfahrplan</h2>
                        <div className="importer-subtitle">
                            Import des Jahresfahrplans
                        </div>
                    </div>

                </section>

                <section className="nfpl-controls">

                    {/* Verzeichnis */}
                    <div className="nfpl-row">
                        <div className="nfpl-row-left">
                            <label>Verzeichnis</label>
                            <input
                                type="text"
                                value="c:/Users/mail/Documents/DBNetz/import"
                                readOnly
                            />
                        </div>

                        <div className="nfpl-row-right">
                            <button className="btn">
                                <i className="fa-solid fa-magnifying-glass"/> Scannen
                            </button>
                        </div>
                    </div>

                    {/* Filter + Actions */}
                    <div className="nfpl-row">
                        <div className="nfpl-row-left">

                        </div>
                        <div className="nfpl-row-right">
                            <button className="btn btn-primary">
                                <i className="fa-solid fa-play"/> Start
                            </button>
                            <button className="btn btn-secondary">
                                <i className="fa-solid fa-xmark"/> Stopp
                            </button>
                        </div>
                    </div>

                </section>

                {/* Gesamtfortschritt */}
                <section className="progress-block">
                    <div className="progress-label">
                        <span>Gesamtfortschritt</span>
                        <span>0%</span>
                    </div>
                    <div className="progress-bar">
                        <div className="progress-value" style={{width: "0%"}}/>
                    </div>
                </section>

                {/* Threads */}
                <section className="netzfahrplan-threads">
                    <h3>Import-Threads (4)</h3>

                    <div className="thread-grid">
                        <div className="thread-card">
                            <div className="thread-header">
                                <span>Import Thread</span>
                                <span className="badge status-ready">Bereit</span>
                            </div>

                            <div className="thread-status">Warte auf Start</div>
                            <div className="thread-count">0 / 12.000 Datensätze</div>

                            <div className="progress-bar small">
                                <div className="progress-value" style={{width: "0%"}}/>
                            </div>
                        </div>

                        <div className="thread-card">
                            <div className="thread-header">
                                <span>Import Thread</span>
                                <span className="badge status-ready">Bereit</span>
                            </div>

                            <div className="thread-status">Warte auf Start</div>
                            <div className="thread-count">0 / 9.500 Datensätze</div>

                            <div className="progress-bar small">
                                <div className="progress-value" style={{width: "0%"}}/>
                            </div>
                        </div>
                    </div>
                </section>
            </section>
        </div>
    );
}
