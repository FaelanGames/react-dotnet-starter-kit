import { useEffect, useState } from "react";
import { me, type MeResponse } from "../api/users";
import { useAuth } from "../auth/useAuth";

export function DashboardPage() {
  const { api, logout } = useAuth();
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

  return (
    <div style={{ maxWidth: 720, margin: "48px auto", padding: 16 }}>
      <div style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
        <h1 style={{ margin: 0 }}>Dashboard</h1>
        <button onClick={logout} style={{ padding: "8px 12px" }}>
          Logout
        </button>
      </div>

      <p style={{ color: "#666" }}>
        Protected route. Calls <code>/api/users/me</code>.
      </p>

      {error && <div style={{ color: "crimson" }}>{error}</div>}

      {!error && !data && <div>Loading…</div>}

      {data && (
        <pre
          style={{
            background: "#f5f5f5",
            padding: 12,
            borderRadius: 8,
            overflow: "auto",
          }}
        >
            {JSON.stringify(data, null, 2)}
        </pre>
      )}
    </div>
  );
}
