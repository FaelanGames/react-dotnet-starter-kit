import { type SubmitEvent, useMemo, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { login } from "../api/auth";
import { useAuth } from "../auth/useAuth";

export function LoginPage() {
  const nav = useNavigate();
  const location = useLocation();
  const { api, setToken } = useAuth();

  const from = useMemo(() => {
    const st = location.state as { from?: string } | null;
    return st?.from ?? "/dashboard";
  }, [location.state]);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: SubmitEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);

    try {
      const res = await login(api, email, password);
      setToken(res.accessToken);
      nav(from, { replace: true });
    } catch (err: any) {
      setError(err?.message ?? "Login failed");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ maxWidth: 420, margin: "48px auto", padding: 16 }}>
      <h1 style={{ marginBottom: 8 }}>Login</h1>
      <p style={{ marginTop: 0 }}>
        Don&apos;t have an account? <Link to="/register">Register</Link>
      </p>

      <form onSubmit={onSubmit}>
        <label>
          Email
          <input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            type="email"
            autoComplete="email"
            style={{ width: "100%", padding: 8, marginTop: 4, marginBottom: 12 }}
          />
        </label>

        <label>
          Password
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            autoComplete="current-password"
            style={{ width: "100%", padding: 8, marginTop: 4, marginBottom: 12 }}
          />
        </label>

        {error && (
          <div style={{ marginBottom: 12, color: "crimson" }}>{error}</div>
        )}

        <button disabled={busy} style={{ width: "100%", padding: 10 }}>
          {busy ? "Signing in..." : "Sign in"}
        </button>
      </form>
    </div>
  );
}
