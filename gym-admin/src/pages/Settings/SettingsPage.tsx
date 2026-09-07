import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type Settings = { id: number; gymName: string; logoUrl: string; tagline: string; websiteUrl: string; currency: string };
type Form = Omit<Settings, "id">;
const emptyForm: Form = { gymName: "", logoUrl: "", tagline: "", websiteUrl: "", currency: "INR" };

export function SettingsPage() {
  const { hasPermission } = useAuth();
  const [id, setId] = useState<number | null>(null); const [form, setForm] = useState<Form>(emptyForm);
  const [loading, setLoading] = useState(true); const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canView = hasPermission(Permission.SettingsView); const canEdit = hasPermission(Permission.SettingsEdit);
  useEffect(() => { if (!canView) { setLoading(false); return; } void (async () => { try { const response = await fetch(`${import.meta.env.VITE_API_URL ?? "http://localhost:5182"}/api/gym-settings`); if (response.status === 404) return; if (!response.ok) throw new Error("Unable to load gym settings."); const data = await response.json() as Settings; setId(data.id); setForm({ gymName: data.gymName, logoUrl: data.logoUrl, tagline: data.tagline, websiteUrl: data.websiteUrl, currency: data.currency }); } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load gym settings." }); } finally { setLoading(false); } })(); }, [canView]);
  function update<K extends keyof Form>(key: K, value: Form[K]) { setForm((current) => ({ ...current, [key]: value })); }
  async function save() { if (!form.gymName.trim()) { setMessage({ type: "error", text: "Gym name is required." }); return; } if (!canEdit) { setMessage({ type: "error", text: "You do not have permission to edit gym settings." }); return; } setSaving(true); try { const result = await apiFetch<Settings>(id ? `/api/gym-settings/${id}` : "/api/gym-settings", { method: id ? "PUT" : "POST", body: JSON.stringify(form) }); setId(result.id); setForm(result); setMessage({ type: "success", text: "Gym settings saved successfully." }); } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save gym settings." }); } finally { setSaving(false); } }
  if (!canView) return <main className="page"><h1>Settings</h1><p>You do not have permission to view this page.</p></main>;
  return <main className="page cms-page"><div className="page-header"><div><h1>Gym Settings</h1><p className="muted">Manage global website configuration.</p></div></div>{message ? <p className={`info ${message.type}`}>{message.text}</p> : null}{loading ? <p>Loading gym settings…</p> : <section className="card cms-form cms-single-form"><div className="form-grid"><div className="form-field"><label>Gym Name<input value={form.gymName} onChange={(event) => update("gymName", event.target.value)} /></label></div><div className="form-field"><label>Tagline<input value={form.tagline} onChange={(event) => update("tagline", event.target.value)} /></label></div><div className="form-field"><label>Website URL<input value={form.websiteUrl} onChange={(event) => update("websiteUrl", event.target.value)} /></label></div><div className="form-field"><label>Currency<input value={form.currency} onChange={(event) => update("currency", event.target.value)} /></label></div></div><div className="form-field"><label>Logo URL<input value={form.logoUrl} onChange={(event) => update("logoUrl", event.target.value)} /></label>{form.logoUrl ? <img className="settings-logo-preview" src={form.logoUrl} alt="Gym logo preview" /> : null}</div><div className="cms-form-footer cms-form-footer-end"><button className="btn" onClick={save} disabled={saving || !canEdit}>{saving ? "Saving…" : "Save Changes"}</button></div></section>}</main>;
}
