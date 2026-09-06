import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type AboutRecord = {
  id: number;
  title: string;
  subtitle: string;
  description: string;
  imageUrl: string;
  experienceYears: number;
  membersCount: number;
  trainersCount: number;
};

type AboutForm = Omit<AboutRecord, "id">;
const emptyForm: AboutForm = { title: "", subtitle: "", description: "", imageUrl: "", experienceYears: 0, membersCount: 0, trainersCount: 0 };
const maxImageDataLength = 10_000_000;

export function AboutPage() {
  const { hasPermission } = useAuth();
  const [recordId, setRecordId] = useState<number | null>(null);
  const [form, setForm] = useState<AboutForm>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canView = hasPermission(Permission.AboutView);
  const canCreate = hasPermission(Permission.AboutCreate);
  const canEdit = hasPermission(Permission.AboutEdit);

  useEffect(() => {
    if (!canView) {
      setLoading(false);
      return;
    }
    void (async () => {
      try {
        const response = await fetch(`${import.meta.env.VITE_API_URL ?? "http://localhost:5182"}/api/about`);
        if (response.status === 404) return;
        if (!response.ok) throw new Error("Unable to load About content.");
        const data = await response.json() as AboutRecord;
        setRecordId(data.id);
        setForm(data);
      } catch (error) {
        setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load About content." });
      } finally {
        setLoading(false);
      }
    })();
  }, [canView]);

  function update<K extends keyof AboutForm>(key: K, value: AboutForm[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  function handleImageUpload(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;
    if (!file.type.startsWith("image/")) {
      setMessage({ type: "error", text: "Please choose a valid image file." });
      return;
    }
    if (file.size > 7 * 1024 * 1024) {
      setMessage({ type: "error", text: "Image must be 7 MB or smaller." });
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const result = typeof reader.result === "string" ? reader.result : "";
      if (result.length > maxImageDataLength) {
        setMessage({ type: "error", text: "Image data is too large. Choose a smaller image." });
        return;
      }
      update("imageUrl", result);
      setMessage({ type: "success", text: "Image ready to save." });
    };
    reader.onerror = () => setMessage({ type: "error", text: "Unable to read selected image." });
    reader.readAsDataURL(file);
  }

  async function save() {
    if (!form.title.trim() || !form.description.trim()) {
      setMessage({ type: "error", text: "Title and description are required." });
      return;
    }
    if ([form.experienceYears, form.membersCount, form.trainersCount].some((value) => value < 0)) {
      setMessage({ type: "error", text: "Numeric values cannot be negative." });
      return;
    }
    if (form.imageUrl.length > maxImageDataLength) {
      setMessage({ type: "error", text: "Image data is too large. Choose a smaller image." });
      return;
    }
    const canSave = recordId ? canEdit : canCreate;
    if (!canSave) {
      setMessage({ type: "error", text: "You do not have permission to save About content." });
      return;
    }
    setSaving(true);
    try {
      const result = await apiFetch<AboutRecord>(recordId ? `/api/about/${recordId}` : "/api/about", {
        method: recordId ? "PUT" : "POST",
        body: JSON.stringify({ ...form, isActive: true }),
      });
      setRecordId(result.id);
      setForm(result);
      setMessage({ type: "success", text: "About content saved successfully." });
    } catch (error) {
      setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save About content." });
    } finally {
      setSaving(false);
    }
  }

  if (!canView) return <main className="page"><h1>About</h1><p>You do not have permission to view this page.</p></main>;

  return (
    <main className="page cms-page">
      <div className="page-header">
        <div><h1>About</h1><p className="muted">Manage the single public About section.</p></div>
      </div>
      {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}
      {loading ? <p>Loading About content…</p> : (
        <section className="card cms-form cms-single-form">
          <div className="form-grid">
            <div className="form-field"><label>Title<input value={form.title} onChange={(event) => update("title", event.target.value)} /></label></div>
            <div className="form-field"><label>Subtitle<input value={form.subtitle} onChange={(event) => update("subtitle", event.target.value)} /></label></div>
          </div>
          <div className="form-field"><label>Description<textarea rows={7} value={form.description} onChange={(event) => update("description", event.target.value)} /></label></div>
          <div className="form-field">
            <label>Image URL</label>
            <div className="image-url-row">
              <input value={form.imageUrl} onChange={(event) => update("imageUrl", event.target.value)} placeholder="https://example.com/about.jpg" />
              <label className="btn btn-secondary upload-button"><input type="file" accept="image/*" onChange={handleImageUpload} />Upload</label>
            </div>
            <small className="muted">JPG, PNG or WebP, up to 7 MB.</small>
          </div>
          <div className="form-grid stats-inputs">
            <div className="form-field"><label>Experience Years<input type="number" min="0" value={form.experienceYears} onChange={(event) => update("experienceYears", Number(event.target.value))} /></label></div>
            <div className="form-field"><label>Members Count<input type="number" min="0" value={form.membersCount} onChange={(event) => update("membersCount", Number(event.target.value))} /></label></div>
            <div className="form-field"><label>Trainers Count<input type="number" min="0" value={form.trainersCount} onChange={(event) => update("trainersCount", Number(event.target.value))} /></label></div>
          </div>
          {form.imageUrl ? <img className="cms-preview-image" src={form.imageUrl} alt="About preview" /> : null}
          <div className="cms-form-footer cms-form-footer-end">
            <button className="btn" onClick={save} disabled={saving || (recordId ? !canEdit : !canCreate)}>{saving ? "Saving…" : recordId ? "Save Changes" : "Create About"}</button>
          </div>
        </section>
      )}
    </main>
  );
}
