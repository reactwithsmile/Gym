import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type GalleryItem = {
  id: number;
  title: string;
  description: string;
  imageUrl: string;
  category: string;
  displayOrder: number;
  isActive: boolean;
};
type GalleryForm = Omit<GalleryItem, "id">;
const emptyForm: GalleryForm = { title: "", description: "", imageUrl: "", category: "", displayOrder: 0, isActive: true };
const maxImageDataLength = 10_000_000;

export function GalleryPage() {
  const { hasPermission } = useAuth();
  const [records, setRecords] = useState<GalleryItem[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [form, setForm] = useState<GalleryForm>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canView = hasPermission(Permission.GalleryManage);
  const canCreate = hasPermission(Permission.GalleryCreate);
  const canEdit = hasPermission(Permission.GalleryEdit);
  const canDelete = hasPermission(Permission.GalleryDelete);

  useEffect(() => {
    if (!canView) { setLoading(false); return; }
    void (async () => {
      try { setRecords(await apiFetch<GalleryItem[]>("/api/gallery/all")); }
      catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load gallery." }); }
      finally { setLoading(false); }
    })();
  }, [canView]);

  function update<K extends keyof GalleryForm>(key: K, value: GalleryForm[K]) { setForm((current) => ({ ...current, [key]: value })); }
  function select(id: number) {
    const item = records.find((record) => record.id === id);
    if (item) { setSelectedId(id); setForm(item); setMessage(null); }
  }
  function upload(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]; event.target.value = ""; if (!file) return;
    if (!file.type.startsWith("image/") || file.size > 7 * 1024 * 1024) {
      setMessage({ type: "error", text: "Choose an image file up to 7 MB." }); return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const result = typeof reader.result === "string" ? reader.result : "";
      if (result.length <= maxImageDataLength) update("imageUrl", result);
      else setMessage({ type: "error", text: "Image data is too large." });
    };
    reader.onerror = () => setMessage({ type: "error", text: "Unable to read image." });
    reader.readAsDataURL(file);
  }
  async function save() {
    if (!form.title.trim() || !form.imageUrl.trim()) { setMessage({ type: "error", text: "Title and image URL are required." }); return; }
    if (form.displayOrder < 0 || form.imageUrl.length > maxImageDataLength) { setMessage({ type: "error", text: "Check the image size and display order." }); return; }
    if (selectedId ? !canEdit : !canCreate) { setMessage({ type: "error", text: "You do not have permission to save gallery items." }); return; }
    setSaving(true);
    try {
      const result = await apiFetch<GalleryItem>(selectedId ? `/api/gallery/${selectedId}` : "/api/gallery", { method: selectedId ? "PUT" : "POST", body: JSON.stringify(form) });
      setRecords((current) => (selectedId ? current.map((item) => item.id === result.id ? result : item) : [...current, result]).sort((a, b) => a.displayOrder - b.displayOrder));
      setSelectedId(result.id); setForm(result); setMessage({ type: "success", text: "Gallery item saved successfully." });
    } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save gallery item." }); }
    finally { setSaving(false); }
  }
  async function remove() {
    if (!selectedId || !canDelete || !window.confirm("Delete this gallery item?")) return;
    try {
      await apiFetch(`/api/gallery/${selectedId}`, { method: "DELETE" });
      setRecords((current) => current.filter((item) => item.id !== selectedId)); setSelectedId(null); setForm(emptyForm);
      setMessage({ type: "success", text: "Gallery item deleted." });
    } catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to delete gallery item." }); }
  }

  if (!canView) return <main className="page"><h1>Gallery</h1><p>You do not have permission to view this page.</p></main>;
  return <main className="page cms-page">
    <div className="page-header"><div><h1>Gallery</h1><p className="muted">Manage the visual stories shown on the public website.</p></div></div>
    {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}
    {loading ? <p>Loading gallery…</p> : <div className="cms-layout">
      <aside className="card cms-list">
        <button className="btn" onClick={() => { setSelectedId(null); setForm(emptyForm); setMessage(null); }} disabled={!canCreate}>New Item</button>
        {records.map((item) => <button className={`cms-list-item${item.id === selectedId ? " selected" : ""}`} key={item.id} onClick={() => select(item.id)}><span>{item.displayOrder}. {item.title}</span><small>{item.isActive ? "Active" : "Inactive"}</small></button>)}
      </aside>
      <section className="card cms-form">
        <div className="form-grid">
          <div className="form-field"><label>Title<input value={form.title} onChange={(event) => update("title", event.target.value)} /></label></div>
          <div className="form-field"><label>Category<input placeholder="Workout, Equipment, Classes…" value={form.category} onChange={(event) => update("category", event.target.value)} /></label></div>
        </div>
        <div className="form-field"><label>Description<textarea rows={4} value={form.description} onChange={(event) => update("description", event.target.value)} /></label></div>
        <div className="form-field"><label>Image URL</label><div className="image-url-row"><input value={form.imageUrl} onChange={(event) => update("imageUrl", event.target.value)} /><label className="btn btn-secondary upload-button"><input type="file" accept="image/*" onChange={upload} />Upload</label></div></div>
        <div className="form-grid stats-inputs">
          <div className="form-field"><label>Display Order<input type="number" min="0" value={form.displayOrder} onChange={(event) => update("displayOrder", Number(event.target.value))} /></label></div>
          <label className="toggle-row"><input type="checkbox" checked={form.isActive} onChange={(event) => update("isActive", event.target.checked)} /> Active</label>
        </div>
        {form.imageUrl ? <img className="cms-preview-image gallery-preview-image" src={form.imageUrl} alt={`${form.title || "Gallery"} preview`} /> : null}
        <div className="cms-form-footer"><button className="btn" onClick={save} disabled={saving || (selectedId ? !canEdit : !canCreate)}>{saving ? "Saving…" : selectedId ? "Save Changes" : "Create Item"}</button>{selectedId && canDelete ? <button className="btn btn-danger" onClick={remove}>Delete</button> : null}</div>
      </section>
    </div>}
  </main>;
}
