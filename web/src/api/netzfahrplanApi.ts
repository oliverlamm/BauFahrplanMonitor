// src/api/netzfahrplanApi.ts
export async function getNetzfahrplanStatus() {
    const res = await fetch("/api/import/netzfahrplan/status");
    if (!res.ok) throw new Error("Status konnte nicht geladen werden");
    return res.json();
}

export async function scanNetzfahrplan() {
    await fetch("/api/import/netzfahrplan/scan", { method: "POST" });
}

export async function startNetzfahrplan() {
    await fetch("/api/import/netzfahrplan/start", { method: "POST" });
}

export async function cancelNetzfahrplan() {
    await fetch("/api/import/netzfahrplan/cancel", { method: "POST" });
}
