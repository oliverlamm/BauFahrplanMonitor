const API_BASE = "/api";

export async function apiGet<T>(url: string): Promise<T> {
    const res = await fetch(API_BASE + url);
    if (!res.ok) {
        throw new Error(await res.text());
    }
    return res.json();
}

export async function apiPost<T>(
    url: string,
    body?: unknown
): Promise<T> {
    const res = await fetch(API_BASE + url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: body ? JSON.stringify(body) : undefined,
    });

    if (!res.ok) {
        throw new Error(await res.text());
    }

    return res.json();
}
