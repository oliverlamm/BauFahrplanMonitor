import { useEffect, useRef, useState } from "react";
import type { ImportJobState } from "../models/importer-status";

/* ============================================================
 * Typen
 * ============================================================ */
export interface BbpNeoStatus {
    state: ImportJobState;
    currentFile: string | null;

    massnahmenGesamt: number;
    massnahmenFertig: number;

    regelungen: number;
    bve: number;
    aps: number;
    iav: number;

    errors: number;

    startedAt?: string | null;
    finishedAt?: string | null;
}

export interface ImportFileInfo {
    fileName: string;
    sizeBytes: number;
    lastModifiedUtc: string;
}

/* ============================================================
 * Default-Status
 * ============================================================ */
const INITIAL_STATUS: BbpNeoStatus = {
    state: "Idle",
    currentFile: null,

    massnahmenGesamt: 0,
    massnahmenFertig: 0,

    regelungen: 0,
    bve: 0,
    aps: 0,
    iav: 0,

    errors: 0,
    startedAt: null,
    finishedAt: null
};

/* ============================================================
 * Hook
 * ============================================================ */
export function useBbpNeo() {
    const [status, setStatus] = useState<BbpNeoStatus>(INITIAL_STATUS);
    const [files, setFiles] = useState<ImportFileInfo[]>([]);
    const [selectedFile, setSelectedFile] = useState<string | null>(null);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const pollRef = useRef<number | null>(null);

    /* ---------------- Status ---------------- */
    const loadStatus = async () => {
        try {
            const res = await fetch("/api/import/bbpneo/status");
            if (!res.ok) throw new Error("Status konnte nicht geladen werden");

            const data = (await res.json()) as BbpNeoStatus;
            setStatus(data);
        } catch (e: any) {
            setError(e.message);
        }
    };

    /* ---------------- Dateien ---------------- */
    const loadFiles = async () => {
        try {
            const res = await fetch("/api/import/bbpneo/files");
            if (!res.ok) throw new Error("Dateiliste konnte nicht geladen werden");

            setFiles(await res.json());
        } catch (e: any) {
            setError(e.message);
        }
    };

    /* ---------------- Polling ---------------- */
    useEffect(() => {
        loadStatus();
        loadFiles();

        pollRef.current = window.setInterval(loadStatus, 2000);

        return () => {
            if (pollRef.current) {
                clearInterval(pollRef.current);
                pollRef.current = null;
            }
        };
    }, []);

    /* ---------------- Start ---------------- */
    const startImport = async () => {
        if (!selectedFile) {
            setError("Keine Datei ausgewählt");
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const res = await fetch("/api/import/bbpneo/start", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ fileName: selectedFile })
            });

            if (!res.ok) {
                const txt = await res.text();
                throw new Error(txt || "Start fehlgeschlagen");
            }

            await loadStatus();
        } catch (e: any) {
            setError(e.message);
        } finally {
            setLoading(false);
        }
    };

    /* ---------------- Cancel ---------------- */
    const cancelImport = async () => {
        await fetch("/api/import/bbpneo/cancel", { method: "POST" });
        await loadStatus();
    };

    return {
        status,
        files,
        selectedFile,
        setSelectedFile,

        loading,
        error,

        startImport,
        cancelImport,
        reloadFiles: loadFiles
    };
}
