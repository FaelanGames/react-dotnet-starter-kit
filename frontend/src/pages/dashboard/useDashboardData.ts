import { useEffect, useState } from "react";
import { me, type MeResponse } from "../../api/users";
import type { ApiClient } from "../../api/client";

type DashboardDataState = {
  data: MeResponse | null;
  error: string | null;
};

export function useDashboardData(api: ApiClient): DashboardDataState {
  const [data, setData] = useState<MeResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;

    (async () => {
      try {
        const res = await me(api);
        if (!alive) return;
        setData(res);
      } catch (err: any) {
        if (!alive) return;
        setError(err?.message ?? "Failed to load user");
      }
    })();

    return () => {
      alive = false;
    };
  }, [api]);

  return { data, error };
}
