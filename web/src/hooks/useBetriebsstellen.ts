import { useEffect, useState } from "react";
import type {
    BetriebsstellenListRow,
    BetriebsstelleDetail
} from "../models/betriebsstelle";

type Lookups = {
    zustaende: string[];
    typen: { id: number; name: string }[];
    regionen: { id: number; name: string }[];
    netzbezirke: { id: number; name: string }[];
};

export function useBetriebsstellen() {
    const [list, setList] = useState<BetriebsstellenListRow[]>([]);
    const [detail, setDetail] = useState<BetriebsstelleDetail | null>(null);
    const [selectedId, setSelectedId] = useState<number | null>(null);

    const [lookups, setLookups] = useState<Lookups>({
        zustaende: [],
        typen: [],
        regionen: [],
        netzbezirke: []
    });

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    /* =====================================================
     * Lookups (GENAU EINMAL)
     * ===================================================== */
    useEffect(() => {
        fetch("/api/admin/basis/lookups")
            .then(r => {
                if (!r.ok) throw new Error();
                return r.json();
            })
            .then(setLookups)
            .catch(() =>
                setError("Lookup-Daten konnten nicht geladen werden")
            );
    }, []);

    /* =====================================================
     * Liste laden
     * ===================================================== */
    const loadList = async (basis?: string) => {
        setLoading(true);
        setError(null);

        try {
            const qs = basis ? `?basis=${basis}` : "";
            const res = await fetch(
                `/api/admin/basis/betriebsstellen/list${qs}`
            );

            if (!res.ok)
                throw new Error("Liste konnte nicht geladen werden");

            setList(await res.json());
        } catch (e: any) {
            setError(e.message ?? "Unbekannter Fehler");
        } finally {
            setLoading(false);
        }
    };

    /* =====================================================
     * Detail laden
     * ===================================================== */
    const loadDetail = async (id: number) => {
        setLoading(true);
        setError(null);

        try {
            const res = await fetch(
                `/api/admin/basis/betriebsstellen/${id}`
            );

            if (!res.ok)
                throw new Error("Detail konnte nicht geladen werden");

            setDetail(await res.json());
            setSelectedId(id);
        } catch (e: any) {
            setError(e.message ?? "Unbekannter Fehler");
        } finally {
            setLoading(false);
        }
    };

    const saveDetail = async (local: BetriebsstelleDetail) => {
        setLoading(true);
        try {
            await fetch(`/api/admin/basis/betriebsstellen/${local.id}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    name: local.name,
                    zustand: local.zustand,
                    typId: local.typId,
                    regionId: local.regionId,
                    netzbezirkId: local.netzbezirkId
                })
            });

            // 🔑 Detail neu laden → sauberer Reset
            await loadDetail(local.id);
        } catch {
            setError("Speichern fehlgeschlagen");
        } finally {
            setLoading(false);
        }
    };
    
    /* =====================================================
     * Initial: Liste laden
     * ===================================================== */
    useEffect(() => {
        loadList();
    }, []);

    return {
        list,
        detail,
        selectedId,
        lookups,

        loadList,
        loadDetail,

        loading,
        error,

        saveDetail
    };
}
