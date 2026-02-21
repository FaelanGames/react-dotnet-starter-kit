import { useAuth } from "../../auth/useAuth";
import { useDashboardData } from "./useDashboardData";

export function DashboardPage() {
  const { api, logout } = useAuth();
  const { data, error } = useDashboardData(api);

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
