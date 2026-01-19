import { useCallback, useEffect, useRef, useState } from "react";

import {
    getNetzfahrplanStatus,
    scanNetzfahrplan,
    startNetzfahrplan,
    cancelNetzfahrplan
} from "../api/netzfahrplanApi";

import type { NetzfahrplanStatus } from "../models/nfpl-status";

export function useNetzfahrplan() {
    const [status, setStatus] = useState<NetzfahrplanStatus | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const pollingRef = useRef<number | null>(null);

    // ------------------------------------------------------------
    // Status laden
    // ------------------------------------------------------------
    const loadStatus = useCallback(async () => {
        try {
            const s = await getNetzfahrplanStatus();
            setStatus(s);

            // Polling stoppen, wenn fertig
            if (
                s.state !== "Running" &&
                s.state !== "Scanning" &&
                pollingRef.current
            ) {
                clearInterval(pollingRef.current);
                pollingRef.current = null;
            }
        } catch (e) {
            console.error("NFPL Status Fehler", e);
            setError("Status konnte nicht geladen werden");
        }
    }, []);

    // ------------------------------------------------------------
    // Polling starten
    // ------------------------------------------------------------
    const startPolling = useCallback(() => {
        if (pollingRef.current) return;
        pollingRef.current = window.setInterval(loadStatus, 2000);
    }, [loadStatus]);

    // ------------------------------------------------------------
    // Initialer Status
    // ------------------------------------------------------------
    useEffect(() => {
        loadStatus();

        return () => {
            if (pollingRef.current) {
                clearInterval(pollingRef.current);
            }
        };
    }, [loadStatus]);

    // ------------------------------------------------------------
    // Scan
    // ------------------------------------------------------------
    const scan = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            await scanNetzfahrplan();
            await loadStatus();
            startPolling();
        } catch (e) {
            console.error("NFPL Scan Fehler", e);
            setError("Scan fehlgeschlagen");
        } finally {
            setLoading(false);
        }
    }, [loadStatus, startPolling]);

    // ------------------------------------------------------------
    // Import starten
    // ------------------------------------------------------------
    const startImport = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            await startNetzfahrplan();
            await loadStatus();
            startPolling();
        } catch (e) {
            console.error("NFPL Start Fehler", e);
            setError("Import konnte nicht gestartet werden");
        } finally {
            setLoading(false);
        }
    }, [loadStatus, startPolling]);

    // ------------------------------------------------------------
    // Abbrechen
    // ------------------------------------------------------------
    const cancelImport = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            await cancelNetzfahrplan();
            await loadStatus();
        } catch (e) {
            console.error("NFPL Cancel Fehler", e);
            setError("Import konnte nicht abgebrochen werden");
        } finally {
            setLoading(false);
        }
    }, [loadStatus]);

    return {
        status,
        loading,
        error,
        scan,
        startImport,
        cancelImport
    };
}
