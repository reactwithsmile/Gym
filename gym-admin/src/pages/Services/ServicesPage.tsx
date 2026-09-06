import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type ServiceRecord = {
  id: number; name: string; shortDescription: string; description: string;
  imageUrl: string; icon: string; isActive: boolean; displayOrder: number;
};
type ServiceForm = Omit<ServiceRecord, "id">;
const emptyForm: ServiceForm = { name: "", shortDescription: "", description: "", imageUrl: "", icon: "", isActive: true, displayOrder: 0 };

export function ServicesPage() {
  const { hasPermission } = useAuth();
  const [records, setRecords] = useState<ServiceRecord[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [form, setForm] = useState<ServiceForm>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canView = hasPermission(Permission.ServicesView);
  const canCreate = hasPermission(Permission.ServicesCreate);
  const canEdit = hasPermission(Permission.ServicesEdit);
  const canDelete = hasPermission(Permission.ServicesDelete);

  useEffect(() => {
    if (!canView) { setLoading(false); return; }
    void (async () => {
      try { setRecords(await apiFetch<ServiceRecord[]>("/api/services/all")); }
      catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load services." }); }
      finally { setLoading(false); }
    })();
  }, [canView]);

  function select(id: number) {
    const record = records.find((item) => item.id === id);
    if (record) { setSelectedId(id); setForm(record); setMessage(null); }
  }
  function update<K extends keyof ServiceForm>(key: K, value: ServiceForm[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }
  async function save() {
    if (!form.name.trim() || !form.shortDescription.trim()) {
      setMessage({ type: "error", text: "Name and short description are required." }); return;
    }
    if (form.displayOrder < 0) { setMessage({ type: "error", text: "Display order cannot be negative." }); return; }
    if (selectedId ? !canEdit : !canCreate) {
      setMessage({ type: "error", text: "You do not have permission to save services." }); return;
    }
    setSaving(true);
    try {
      const result = await apiFetch<ServiceRecord>(selectedId ? `/api/services/${selectedId}` : "/api/services", {
        method: selectedId ? "PUT" : "POST", body: JSON.stringify(form)
      });
      setRecords((current) => selectedId ? current.map((item) => item.id === result.id ? result : item) : [...current, result].sort((a, b) => a.displayOrder - b.displayOrder));
      setSelectedId(result.id); setForm(result); setMessage({ type: "success", text: "Service saved successfully." });
    } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save service." }); }
    finally { setSaving(false); }
  }
  async function remove() {
    if (!selectedId || !canDelete || !window.confirm("Delete this service?")) return;
    try {
      await apiFetch(`/api/services/${selectedId}`, { method: "DELETE" });
      const remaining = records.filter((item) => item.id !== selectedId);
      setRecords(remaining); setSelectedId(null); setForm(emptyForm); setMessage({ type: "success", text: "Service deleted successfully." });
    } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to delete service." }); }
  }
  function upload(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]; event.target.value = ""; if (!file) return;
    if (!file.type.startsWith("image/") || file.size > 7 * 1024 * 1024) {
      setMessage({ type: "error", text: "Choose an image file up to 7 MB." }); return;
    }
    const reader = new FileReader();
    reader.onload = () => update("imageUrl", typeof reader.result === "string" ? reader.result : "");
    reader.onerror = () => setMessage({ type: "error", text: "Unable to read image." });
    reader.readAsDataURL(file);
  }

  if (!canView) return <main className="page"><h1>Services</h1><p>You do not have permission to view this page.</p></main>;
  return <main className="page cms-page">
    <div className="page-header"><div><h1>Services</h1><p className="muted">Manage services displayed on the public website.</p></div></div>
    {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}
    {loading ? <p>Loading services…</p> : <div className="cms-layout">
      <aside className="card cms-list">
        <button className="btn" onClick={() => { setSelectedId(null); setForm(emptyForm); setMessage(null); }} disabled={!canCreate}>New Service</button>
        {records.map((record) => <button key={record.id} className={`cms-list-item${record.id === selectedId ? " selected" : ""}`} onClick={() => select(record.id)}>
          <span>{record.displayOrder}. {record.name}</span><small>{record.isActive ? "Active" : "Inactive"}</small>
        </button>)}
      </aside>
      <section className="card cms-form">
        <div className="form-grid">
          <div className="form-field"><label>Name<input value={form.name} onChange={(event) => update("name", event.target.value)} /></label></div>
          <div className="form-field"><label>Icon<input value={form.icon} onChange={(event) => update("icon", event.target.value)} placeholder="dumbbell" /></label></div>
        </div>
        <div className="form-field"><label>Short Description<input value={form.shortDescription} onChange={(event) => update("shortDescription", event.target.value)} /></label></div>
        <div className="form-field"><label>Description<textarea rows={5} value={form.description} onChange={(event) => update("description", event.target.value)} /></label></div>
        <div className="form-field"><label>Image URL</label><div className="image-url-row"><input value={form.imageUrl} onChange={(event) => update("imageUrl", event.target.value)} /><label className="btn btn-secondary upload-button"><input type="file" accept="image/*" onChange={upload} />Upload</label></div></div>
        <div className="form-grid stats-inputs">
          <div className="form-field"><label>Display Order<input type="number" min="0" value={form.displayOrder} onChange={(event) => update("displayOrder", Number(event.target.value))} /></label></div>
          <label className="toggle-row"><input type="checkbox" checked={form.isActive} onChange={(event) => update("isActive", event.target.checked)} /> Active</label>
        </div>
        {form.imageUrl ? <img className="cms-preview-image" src={form.imageUrl} alt={`${form.name} preview`} /> : null}
        <div className="cms-form-footer"><button className="btn" onClick={save} disabled={saving || (selectedId ? !canEdit : !canCreate)}>{saving ? "Saving…" : selectedId ? "Save Changes" : "Create Service"}</button>{selectedId && canDelete ? <button className="btn btn-danger" onClick={remove}>Delete</button> : null}</div>
      </section>
    </div>}
  </main>;
}
