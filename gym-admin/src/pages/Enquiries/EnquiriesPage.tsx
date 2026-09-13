import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type Enquiry = { id: number; name: string; email: string; phone: string; message: string; status: string; reason: string; createdAt: string; updatedAt: string };
const statuses = ["New", "Contacted", "Converted", "Closed", "Spam"];

export function EnquiriesPage() {
  const { hasPermission } = useAuth();
  const [items, setItems] = useState<Enquiry[]>([]);
  const [selected, setSelected] = useState<Enquiry | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const canView = hasPermission(Permission.EnquiriesView);
  const canEdit = hasPermission(Permission.EnquiriesEdit);
  useEffect(() => { if (canView) void load(); else setLoading(false); }, [canView]);
  async function load() { try { setItems(await apiFetch<Enquiry[]>("/api/enquiries")); } catch (error) { setMessage(error instanceof Error ? error.message : "Unable to load enquiries."); } finally { setLoading(false); } }
  async function save() {
    if (!selected || !canEdit) return;
    if (selected.status === "Closed" && !selected.reason.trim()) { setMessage("Reason is required when closing an enquiry."); return; }
    setSaving(true);
    try { const updated = await apiFetch<Enquiry>(`/api/enquiries/${selected.id}`, { method: "PUT", body: JSON.stringify({ status: selected.status, reason: selected.reason }) }); setItems((current) => current.map((item) => item.id === updated.id ? updated : item)); setSelected(updated); setMessage("Enquiry updated."); } catch (error) { setMessage(error instanceof Error ? error.message : "Unable to update enquiry."); } finally { setSaving(false); }
  }
  if (!canView) return <main className="page"><h1>Enquiries</h1><p>You do not have permission to view enquiries.</p></main>;
  return <main className="page cms-page"><div className="page-header"><div><p className="eyebrow">LEAD MANAGEMENT</p><h1>Enquiries</h1><p className="muted">Review website enquiries and track their follow-up status.</p></div></div>{message ? <p className="info">{message}</p> : null}{loading ? <p>Loading enquiries…</p> : <div className="cms-layout"><aside className="card cms-list"><strong>{items.length} enquiries</strong>{items.map((item) => <button className={`cms-list-item${selected?.id === item.id ? " selected" : ""}`} key={item.id} onClick={() => { setSelected(item); setMessage(""); }}><span>{item.name}</span><small>{item.status}</small></button>)}</aside><section className="card cms-form">{selected ? <><p className="eyebrow">ENQUIRY #{selected.id}</p><h2>{selected.name}</h2><p className="muted">{selected.email}{selected.phone ? ` • ${selected.phone}` : ""}</p><div className="enquiry-message">{selected.message}</div><div className="form-field"><label>Status<select value={selected.status} disabled={!canEdit} onChange={(event) => setSelected({ ...selected, status: event.target.value })}>{statuses.map((status) => <option key={status}>{status}</option>)}</select></label></div><div className="form-field"><label>Reason / notes<textarea rows={4} value={selected.reason} disabled={!canEdit} placeholder="Reason for closing or follow-up notes" onChange={(event) => setSelected({ ...selected, reason: event.target.value })} /></label></div><p className="muted">Received {new Date(selected.createdAt).toLocaleString()}</p>{canEdit ? <button className="btn" disabled={saving} onClick={() => void save()}>{saving ? "Saving…" : "Save status"}</button> : null}</> : <p className="empty-state">Select an enquiry to view details.</p>}</section></div>}</main>;
}
