import { useCallback, useEffect, useState } from "react";

import {
    getZvFExportStatus,
    startZvFExportImport,
    cancelZvFExportImport,
    scanZvFExport
} from "../api/zvfExportApi";

import type { ZvFExportStatus } from "../api/zvfExportApi";
import type { ZvFFileFilter } from "../api/zvfExportApi";

// =========================================================
// ZvF Export Hook
// =========================================================

export function useZvFExport() {

    const [status, setStatus] = useState<ZvFExportStatus | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    // -----------------------------
    // Status polling
    // -----------------------------
    const refreshStatus = useCallback(async () => {
        try {
            const s = await getZvFExportStatus();
            setStatus(s);
            setError(null);
        } catch (e) {
            setError((e as Error).message);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        refreshStatus();
        const id = setInterval(refreshStatus, 1500);
        return () => clearInterval(id);
    }, [refreshStatus]);

    // -----------------------------
    // Actions
    // -----------------------------
    const startImport = async () => {
        try {
            await startZvFExportImport();
            await refreshStatus();
        } catch (e) {
            setError((e as Error).message);
        }
    };

    const cancelImport = async () => {
        try {
            await cancelZvFExportImport();
            await refreshStatus();
        } catch (e) {
            setError((e as Error).message);
        }
    };

    const scan = async (filter: ZvFFileFilter) => {
        try {
            await scanZvFExport(filter);
            await refreshStatus();
        } catch (e) {
            setError((e as Error).message);
        }
    };

    return {
        status,
        loading,
        error,
        startImport,
        cancelImport,
        scan,
        refreshStatus
    };
}
