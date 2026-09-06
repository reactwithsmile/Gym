import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type HeroRecord = {
  id: number;
  heading: string;
  description: string;
  primaryButtonText: string;
  primaryButtonLink: string;
  secondaryButtonText: string;
  secondaryButtonLink: string;
  imageUrl: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

type HeroFormState = {
  heading: string;
  description: string;
  primaryButtonText: string;
  primaryButtonLink: string;
  secondaryButtonText: string;
  secondaryButtonLink: string;
  imageUrl: string;
  isActive: boolean;
};

const emptyForm = (): HeroFormState => ({
  heading: "",
  description: "",
  primaryButtonText: "",
  primaryButtonLink: "",
  secondaryButtonText: "",
  secondaryButtonLink: "",
  imageUrl: "",
  isActive: true,
});

const toFormState = (hero?: HeroRecord | null): HeroFormState => ({
  heading: hero?.heading ?? "",
  description: hero?.description ?? "",
  primaryButtonText: hero?.primaryButtonText ?? "",
  primaryButtonLink: hero?.primaryButtonLink ?? "",
  secondaryButtonText: hero?.secondaryButtonText ?? "",
  secondaryButtonLink: hero?.secondaryButtonLink ?? "",
  imageUrl: hero?.imageUrl ?? "",
  isActive: hero?.isActive ?? true,
});

export function HeroPage() {
  const { hasPermission } = useAuth();
  const canView = hasPermission(Permission.HeroView);
  const canCreate = hasPermission(Permission.HeroCreate);
  const canEdit = hasPermission(Permission.HeroEdit);
  const canDelete = hasPermission(Permission.HeroDelete);

  const [heroId, setHeroId] = useState<number | null>(null);
  const [form, setForm] = useState<HeroFormState>(emptyForm());
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function loadHero() {
    setLoading(true);
    setMessage(null);

    try {
      const hero = await apiFetch<HeroRecord>("/api/hero");
      setHeroId(hero.id);
      setForm(toFormState(hero));
    } catch (error) {
      const text = error instanceof Error ? error.message : "Unable to load hero.";
      const noActiveHero = /not found|no active hero|not exist/i.test(text);
      setHeroId(null);
      setForm(emptyForm());
      if (!noActiveHero) {
        setMessage({ type: "error", text });
      }
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (canView) {
      void loadHero();
    } else {
      setLoading(false);
      setHeroId(null);
      setForm(emptyForm());
    }
  }, [canView]);

  function handleFieldChange<K extends keyof HeroFormState>(key: K, value: HeroFormState[K]) {
    setForm((current) => ({
      ...current,
      [key]: value,
    }));
  }

  function handleImageUpload(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file) {
      return;
    }

    if (!file.type.startsWith("image/")) {
      setMessage({ type: "error", text: "Please choose a valid image file." });
      event.target.value = "";
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const result = typeof reader.result === "string" ? reader.result : "";
      if (result) {
        handleFieldChange("imageUrl", result);
        setMessage({ type: "success", text: "Image uploaded successfully for preview." });
      }
    };
    reader.onerror = () => {
      setMessage({ type: "error", text: "Unable to read selected image." });
    };
    reader.readAsDataURL(file);
    event.target.value = "";
  }

  function validateForm() {
    if (!form.heading.trim()) {
      return "Heading is required.";
    }

    if (!form.description.trim()) {
      return "Description is required.";
    }

    if (!form.imageUrl.trim()) {
      return "Image URL is required.";
    }

    const hasPrimaryText = !!form.primaryButtonText.trim();
    const hasPrimaryLink = !!form.primaryButtonLink.trim();
    if (hasPrimaryText !== hasPrimaryLink) {
      return "Primary button text and link must be provided together.";
    }

    const hasSecondaryText = !!form.secondaryButtonText.trim();
    const hasSecondaryLink = !!form.secondaryButtonLink.trim();
    if (hasSecondaryText !== hasSecondaryLink) {
      return "Secondary button text and link must be provided together.";
    }

    return "";
  }

  async function handleSave() {
    const validationMessage = validateForm();
    if (validationMessage) {
      setMessage({ type: "error", text: validationMessage });
      return;
    }

    const payload = {
      heading: form.heading.trim(),
      description: form.description.trim(),
      primaryButtonText: form.primaryButtonText.trim(),
      primaryButtonLink: form.primaryButtonLink.trim(),
      secondaryButtonText: form.secondaryButtonText.trim(),
      secondaryButtonLink: form.secondaryButtonLink.trim(),
      imageUrl: form.imageUrl.trim(),
      isActive: form.isActive,
    };

    const hasAccessToWrite = heroId ? canEdit : canCreate;
    if (!hasAccessToWrite) {
      setMessage({ type: "error", text: "You do not have permission to save hero content." });
      return;
    }

    setSaving(true);
    setMessage(null);

    try {
      const result = heroId
        ? await apiFetch<HeroRecord>(`/api/hero/${heroId}`, {
            method: "PUT",
            body: JSON.stringify(payload),
          })
        : await apiFetch<HeroRecord>("/api/hero", {
            method: "POST",
            body: JSON.stringify(payload),
          });

      setHeroId(result.id);
      setForm(toFormState(result));
      setMessage({ type: "success", text: heroId ? "Hero updated successfully." : "Hero created successfully." });
    } catch (error) {
      setMessage({
        type: "error",
        text: error instanceof Error ? error.message : "Unable to save hero content.",
      });
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!heroId || !canDelete) {
      return;
    }

    const confirmed = window.confirm("Delete this hero section?");
    if (!confirmed) {
      return;
    }

    setDeleting(true);
    setMessage(null);

    try {
      await apiFetch(`/api/hero/${heroId}`, { method: "DELETE" });
      setHeroId(null);
      setForm(emptyForm());
      setMessage({ type: "success", text: "Hero deleted successfully." });
    } catch (error) {
      setMessage({
        type: "error",
        text: error instanceof Error ? error.message : "Unable to delete hero content.",
      });
    } finally {
      setDeleting(false);
    }
  }

  if (!canView) {
    return (
      <main className="page hero-page">
        <h1>Hero</h1>
        <p>You do not have permission to view this page.</p>
      </main>
    );
  }

  return (
    <main className="page hero-page">
      <div className="page-header">
        <div>
          <h1>Hero</h1>
          <p className="muted">Manage the homepage hero section.</p>
        </div>
        {heroId && canDelete ? (
          <button className="btn btn-danger" type="button" onClick={handleDelete} disabled={deleting}>
            {deleting ? "Deleting…" : "Delete"}
          </button>
        ) : null}
      </div>

      {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}

      {loading ? (
        <p>Loading hero content…</p>
      ) : (
        <div className="hero-layout">
          <section className="card hero-form-card">
            <div className="form-field">
              <label htmlFor="hero-heading">Heading</label>
              <input
                id="hero-heading"
                value={form.heading}
                onChange={(event) => handleFieldChange("heading", event.target.value)}
                placeholder="Build your strongest self"
              />
            </div>

            <div className="form-field">
              <label htmlFor="hero-description">Description</label>
              <textarea
                id="hero-description"
                value={form.description}
                onChange={(event) => handleFieldChange("description", event.target.value)}
                rows={5}
                placeholder="Write a short description for the homepage hero."
              />
            </div>

            <div className="form-grid">
              <div className="form-field">
                <label htmlFor="hero-primary-text">Primary Button Text</label>
                <input
                  id="hero-primary-text"
                  value={form.primaryButtonText}
                  onChange={(event) => handleFieldChange("primaryButtonText", event.target.value)}
                  placeholder="Join Now"
                />
              </div>

              <div className="form-field">
                <label htmlFor="hero-primary-link">Primary Button Link</label>
                <input
                  id="hero-primary-link"
                  value={form.primaryButtonLink}
                  onChange={(event) => handleFieldChange("primaryButtonLink", event.target.value)}
                  placeholder="/membership"
                />
              </div>
            </div>

            <div className="form-grid">
              <div className="form-field">
                <label htmlFor="hero-secondary-text">Secondary Button Text</label>
                <input
                  id="hero-secondary-text"
                  value={form.secondaryButtonText}
                  onChange={(event) => handleFieldChange("secondaryButtonText", event.target.value)}
                  placeholder="Learn More"
                />
              </div>

              <div className="form-field">
                <label htmlFor="hero-secondary-link">Secondary Button Link</label>
                <input
                  id="hero-secondary-link"
                  value={form.secondaryButtonLink}
                  onChange={(event) => handleFieldChange("secondaryButtonLink", event.target.value)}
                  placeholder="/about"
                />
              </div>
            </div>

            <div className="form-field">
              <label htmlFor="hero-image-url">Image URL</label>
              <div className="image-url-row">
                <input
                  id="hero-image-url"
                  value={form.imageUrl}
                  onChange={(event) => handleFieldChange("imageUrl", event.target.value)}
                  placeholder="https://example.com/hero.jpg"
                />
                <label className="btn btn-secondary upload-button">
                  <input type="file" accept="image/*" onChange={handleImageUpload} />
                  Upload
                </label>
              </div>
            </div>

            <label className="toggle-row">
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(event) => handleFieldChange("isActive", event.target.checked)}
              />
              <span>Active</span>
            </label>

            <div className="hero-actions">
              <button
                className="btn"
                type="button"
                onClick={handleSave}
                disabled={saving || ((!heroId && !canCreate) || (!!heroId && !canEdit))}
              >
                {saving ? "Saving…" : heroId ? "Save Changes" : "Create Hero"}
              </button>
            </div>
          </section>

          <aside className="card hero-preview-card">
            <h3>Preview</h3>
            {form.imageUrl ? (
              <img className="hero-preview-image" src={form.imageUrl} alt="Hero preview" />
            ) : (
              <div className="hero-empty-preview">No image selected</div>
            )}

            <div className="hero-preview-copy">
              <span className={`status-badge ${form.isActive ? "active" : "inactive"}`}>
                {form.isActive ? "Active" : "Inactive"}
              </span>
              <h4>{form.heading || "Hero heading"}</h4>
              <p>{form.description || "Your hero description will appear here."}</p>
              {form.primaryButtonText || form.secondaryButtonText ? (
                <div className="preview-actions">
                  {form.primaryButtonText ? <span>{form.primaryButtonText}</span> : null}
                  {form.secondaryButtonText ? <span>{form.secondaryButtonText}</span> : null}
                </div>
              ) : null}
            </div>
          </aside>
        </div>
      )}
    </main>
  );
}
