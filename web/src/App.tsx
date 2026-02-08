import { useState } from "react";
import {useStatus} from "./hooks/useStatus";

import StatusPage from "./pages/Status/StatusPage";
import NetzfahrplanPage from "./pages/Netzfahrplan/NetzfahrplanPage";
import BbpNeoPage from "./pages/BbpNeo/BbpNeoPage";
import ZvFExportPage from "./pages/ZvFExport/ZvFExportPage";
import BetriebsstellenPage from "./pages/Betriebsstellen/BetriebsstellenPage";
import ZugsichtPage from "./pages/Auswertungen/ZugsichtPage";

import dbInfraGoLogo from "./assets/DBInfrago.png";


type Page =
    | "status"
    | "netzfahrplan"
    | "bbpneo"
    | "zvfexport"
    | "zugsicht"
    | "betriebsstellen"
    | "config";



export default function App() {
    const [activePage, setActivePage] = useState<Page>("status");
    const {data, loading, error} = useStatus();
    if (loading) {
        return <div>Lade Systemstatus…</div>;
    }

    if (error || !data) {
        return <div className="error">Fehler: {error}</div>;
    }
    
    const renderPage = () => {
        switch (activePage) {
            case "status": return <StatusPage />;
            case "netzfahrplan": return <NetzfahrplanPage />;
            case "bbpneo": return <BbpNeoPage />;
            case "zvfexport": return <ZvFExportPage />;
            case "zugsicht": return <ZugsichtPage />;
            case "betriebsstellen": return <BetriebsstellenPage />;
            case "config": return <StatusPage />;
            default: return null;
        }
    };
    data.databaseStatus.status.toLowerCase();
    return (
        <div className="app">
            {/* Header */}
            <header className="header">
                <div className="header-left">
                    <div
                        className="app-title"
                        onClick={() => setActivePage("status")}
                        style={{ cursor: "pointer" }}
                    >
                        {data.name}
                    </div>
                    <div className="app-subtitle">Data Import Dashboard</div>
                </div>

                <div className="header-right">
                    <div className="session">
                        <img
                            src={dbInfraGoLogo}
                            alt="DB InfraGO"
                            className="header-logo"
                        />
                    </div>
                </div>
            </header>

            <div className="body">
                {/* Sidebar */}
                <aside className="sidebar">
                    {/* Importer */}
                    <div className="sidebar-section">
                        <div className="sidebar-title">Importer</div>

                        <nav className="nav">
                            <div
                                className={`nav-item ${
                                    activePage === "netzfahrplan" ? "active" : ""
                                }`}
                                onClick={() => setActivePage("netzfahrplan")}
                            >
                                <i className="fa-solid fa-database" />
                                Netzfahrplan
                            </div>

                            <div
                                className={`nav-item ${
                                    activePage === "bbpneo" ? "active" : ""
                                }`}
                                onClick={() => setActivePage("bbpneo")}
                            >
                                <i className="fa-solid fa-train" />
                                BBP Neo
                            </div>

                            <div
                                className={`nav-item ${
                                    activePage === "zvfexport" ? "active" : ""
                                }`}
                                onClick={() => setActivePage("zvfexport")}
                            >
                                <i className="fa-solid fa-file-lines" />
                                ZvF / ÜB / Fplo
                            </div>
                        </nav>
                    </div>

                    {/* Verwaltung */}
                    <div className="sidebar-section">
                        <div className="sidebar-title">Auswertungen</div>

                        <nav className="nav">
                            <div
                                className={`nav-item ${
                                    activePage === "zugsicht" ? "active" : ""
                                }`}
                                onClick={() => setActivePage("zugsicht")}
                            >
                                <i className="fa-solid fa-chart-line" />
                                Zugsicht
                            </div>
                        </nav>
                    </div>
                    
                    {/* Verwaltung */}
                    <div className="sidebar-section">
                        <div className="sidebar-title">Verwaltung</div>

                        <nav className="nav">
                            <div
                                className={`nav-item ${
                                    activePage === "betriebsstellen" ? "active" : ""
                                }`}
                                onClick={() => setActivePage("betriebsstellen")}
                            >
                                <i className="fa-solid fa-map-location-dot" />
                                Betriebsstellen
                            </div>
                        </nav>
                    </div>

                    {/* Bottom */}
                    <div className="sidebar-bottom">
                        <button
                            className={`nav-item config ${
                                activePage === "config" ? "active" : ""
                            }`}
                            onClick={() => setActivePage("config")}
                        >
                            <i className="fa-solid fa-gear" />
                            Konfiguration
                        </button>
                    </div>
                </aside>

                {/* Content */}
                <main className="content">
                    {renderPage()}
                </main>
            </div>

            {/* Footer */}
            <footer className="footer">
                <div className="footer-left">
                    <span className="footer-app">{data.allgemein.name}</span>
                    <span className="footer-version">v{data.allgemein.version}</span>
                </div>

                <div className="footer-right">
                    <div className="session">
                        <span className="session-label">Session: </span>
                        <span className="session-value">{data.allgemein.machineName}</span>
                    </div>
                </div>
            </footer>
        </div>
    );
}
