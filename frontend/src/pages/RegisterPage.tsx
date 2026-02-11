import { type SubmitEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { register } from "../api/auth";
import { useAuth } from "../auth/useAuth";

export function RegisterPage() {
  const nav = useNavigate();
  const { api, setToken } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: SubmitEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);

    try {
      const res = await register(api, email, password);
      setToken(res.accessToken);
      nav("/dashboard", { replace: true });
    } catch (err: any) {
      if (err?.status === 409) {
        setError("Email is already registered.");
      } else {
        setError(err?.message ?? "Registration failed");
      }
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ maxWidth: 420, margin: "48px auto", padding: 16 }}>
      <h1 style={{ marginBottom: 8 }}>Register</h1>
      <p style={{ marginTop: 0 }}>
        Already have an account? <Link to="/login">Login</Link>
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
          Password (min 8 chars)
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            type="password"
            autoComplete="new-password"
            style={{ width: "100%", padding: 8, marginTop: 4, marginBottom: 12 }}
          />
        </label>

        {error && (
          <div style={{ marginBottom: 12, color: "crimson" }}>{error}</div>
        )}

        <button disabled={busy} style={{ width: "100%", padding: 10 }}>
          {busy ? "Creating..." : "Create account"}
        </button>
      </form>
    </div>
  );
}
