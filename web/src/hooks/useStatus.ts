import { useEffect, useState } from "react";
import { fetchSystemStatus } from "../api/status.api";
import type { SystemStatus } from "../models/system-status";

export function useStatus() {
    const [data, setData] = useState<SystemStatus | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;

        fetchSystemStatus()
            .then((result) => {
                if (!cancelled) setData(result);
            })
            .catch((err) => {
                if (!cancelled) setError(err.message);
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, []);

    return { data, loading, error };
}
