import type { SystemStatus } from "../models/system-status";

export async function fetchSystemStatus(): Promise<SystemStatus> {
    const res = await fetch("/api/status");

    if (!res.ok) {
        throw new Error("Status API nicht erreichbar");
    }

    return res.json();
}
