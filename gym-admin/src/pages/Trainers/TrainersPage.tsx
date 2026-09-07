import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type Trainer = {
  id: number; name: string; role: string; bio: string; imageUrl: string;
  specialization: string; experienceYears: number; instagramUrl: string;
  facebookUrl: string; displayOrder: number; isActive: boolean;
};
type TrainerForm = Omit<Trainer, "id">;
const emptyForm: TrainerForm = { name: "", role: "", bio: "", imageUrl: "", specialization: "", experienceYears: 0, instagramUrl: "", facebookUrl: "", displayOrder: 0, isActive: true };
const maxImageDataLength = 10_000_000;

export function TrainersPage() {
  const { hasPermission } = useAuth();
  const [records, setRecords] = useState<Trainer[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [form, setForm] = useState<TrainerForm>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canView = hasPermission(Permission.TrainersView);
  const canCreate = hasPermission(Permission.TrainersCreate);
  const canEdit = hasPermission(Permission.TrainersEdit);
  const canDelete = hasPermission(Permission.TrainersDelete);

  useEffect(() => {
    if (!canView) { setLoading(false); return; }
    void (async () => {
      try { setRecords(await apiFetch<Trainer[]>("/api/trainers/all")); }
      catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load trainers." }); }
      finally { setLoading(false); }
    })();
  }, [canView]);

  function update<K extends keyof TrainerForm>(key: K, value: TrainerForm[K]) { setForm((current) => ({ ...current, [key]: value })); }
  function select(id: number) { const record = records.find((item) => item.id === id); if (record) { setSelectedId(id); setForm(record); setMessage(null); } }
  function upload(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]; event.target.value = ""; if (!file) return;
    if (!file.type.startsWith("image/") || file.size > 7 * 1024 * 1024) { setMessage({ type: "error", text: "Choose an image file up to 7 MB." }); return; }
    const reader = new FileReader();
    reader.onload = () => { const result = typeof reader.result === "string" ? reader.result : ""; if (result.length <= maxImageDataLength) update("imageUrl", result); else setMessage({ type: "error", text: "Image data is too large." }); };
    reader.onerror = () => setMessage({ type: "error", text: "Unable to read image." });
    reader.readAsDataURL(file);
  }
  async function save() {
    if (!form.name.trim() || !form.role.trim()) { setMessage({ type: "error", text: "Name and role are required." }); return; }
    if (form.experienceYears < 0 || form.displayOrder < 0) { setMessage({ type: "error", text: "Experience years and display order cannot be negative." }); return; }
    if (form.imageUrl.length > maxImageDataLength) { setMessage({ type: "error", text: "Image data is too large." }); return; }
    if (selectedId ? !canEdit : !canCreate) { setMessage({ type: "error", text: "You do not have permission to save trainers." }); return; }
    setSaving(true);
    try {
      const result = await apiFetch<Trainer>(selectedId ? `/api/trainers/${selectedId}` : "/api/trainers", { method: selectedId ? "PUT" : "POST", body: JSON.stringify(form) });
      setRecords((current) => (selectedId ? current.map((item) => item.id === result.id ? result : item) : [...current, result]).sort((a, b) => a.displayOrder - b.displayOrder));
      setSelectedId(result.id); setForm(result); setMessage({ type: "success", text: "Trainer saved successfully." });
    } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save trainer." }); }
    finally { setSaving(false); }
  }
  async function remove() {
    if (!selectedId || !canDelete || !window.confirm("Delete this trainer?")) return;
    try { await apiFetch(`/api/trainers/${selectedId}`, { method: "DELETE" }); setRecords((current) => current.filter((item) => item.id !== selectedId)); setSelectedId(null); setForm(emptyForm); setMessage({ type: "success", text: "Trainer deleted successfully." }); }
    catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to delete trainer." }); }
  }

  if (!canView) return <main className="page"><h1>Trainers</h1><p>You do not have permission to view this page.</p></main>;
  return <main className="page cms-page">
    <div className="page-header"><div><h1>Trainers</h1><p className="muted">Manage the trainers shown on the public website.</p></div></div>
    {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}
    {loading ? <p>Loading trainers…</p> : <div className="cms-layout">
      <aside className="card cms-list">
        <button className="btn" onClick={() => { setSelectedId(null); setForm(emptyForm); setMessage(null); }} disabled={!canCreate}>New Trainer</button>
        {records.map((record) => <button className={`cms-list-item${record.id === selectedId ? " selected" : ""}`} key={record.id} onClick={() => select(record.id)}><span>{record.displayOrder}. {record.name}</span><small>{record.isActive ? "Active" : "Inactive"}</small></button>)}
      </aside>
      <section className="card cms-form">
        <div className="form-grid">
          <div className="form-field"><label>Name<input value={form.name} onChange={(event) => update("name", event.target.value)} /></label></div>
          <div className="form-field"><label>Role<input value={form.role} onChange={(event) => update("role", event.target.value)} /></label></div>
        </div>
        <div className="form-field"><label>Bio<textarea rows={5} value={form.bio} onChange={(event) => update("bio", event.target.value)} /></label></div>
        <div className="form-field"><label>Image URL</label><div className="image-url-row"><input value={form.imageUrl} onChange={(event) => update("imageUrl", event.target.value)} /><label className="btn btn-secondary upload-button"><input type="file" accept="image/*" onChange={upload} />Upload</label></div></div>
        <div className="form-grid">
          <div className="form-field"><label>Specialization<input value={form.specialization} onChange={(event) => update("specialization", event.target.value)} /></label></div>
          <div className="form-field"><label>Experience Years<input type="number" min="0" value={form.experienceYears} onChange={(event) => update("experienceYears", Number(event.target.value))} /></label></div>
        </div>
        <div className="form-grid">
          <div className="form-field"><label>Instagram URL<input value={form.instagramUrl} onChange={(event) => update("instagramUrl", event.target.value)} /></label></div>
          <div className="form-field"><label>Facebook URL<input value={form.facebookUrl} onChange={(event) => update("facebookUrl", event.target.value)} /></label></div>
        </div>
        <div className="form-grid stats-inputs">
          <div className="form-field"><label>Display Order<input type="number" min="0" value={form.displayOrder} onChange={(event) => update("displayOrder", Number(event.target.value))} /></label></div>
          <label className="toggle-row"><input type="checkbox" checked={form.isActive} onChange={(event) => update("isActive", event.target.checked)} /> Active</label>
        </div>
        {form.imageUrl ? <img className="cms-preview-image" src={form.imageUrl} alt={`${form.name} preview`} /> : null}
        <div className="cms-form-footer"><button className="btn" onClick={save} disabled={saving || (selectedId ? !canEdit : !canCreate)}>{saving ? "Saving…" : selectedId ? "Save Changes" : "Create Trainer"}</button>{selectedId && canDelete ? <button className="btn btn-danger" onClick={remove}>Delete</button> : null}</div>
      </section>
    </div>}
  </main>;
}
