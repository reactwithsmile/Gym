import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type Product = { id: number; name: string; category: string; description: string; imageUrl: string; price: number; isActive: boolean; displayOrder: number };
const empty: Omit<Product, "id"> = { name: "", category: "Supplements", description: "", imageUrl: "", price: 0, isActive: true, displayOrder: 0 };

export function ProductsPage() {
  const { hasPermission } = useAuth();
  const [items, setItems] = useState<Product[]>([]); const [selected, setSelected] = useState<Product | null>(null); const [form, setForm] = useState(empty); const [message, setMessage] = useState(""); const [saving, setSaving] = useState(false);
  const canView = hasPermission(Permission.ProductsView), canCreate = hasPermission(Permission.ProductsCreate), canEdit = hasPermission(Permission.ProductsEdit), canDelete = hasPermission(Permission.ProductsDelete);
  async function load() { try { setItems(await apiFetch<Product[]>("/api/products/all")); } catch (error) { setMessage(error instanceof Error ? error.message : "Unable to load products."); } }
  useEffect(() => { if (canView) void load(); }, [canView]);
  function edit(item: Product) { setSelected(item); setForm(item); setMessage(""); }
  function update<K extends keyof typeof empty>(key: K, value: (typeof empty)[K]) { setForm((current) => ({ ...current, [key]: value })); }
  function upload(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;
    if (!file.type.startsWith("image/") || file.size > 7 * 1024 * 1024) {
      setMessage("Choose an image file up to 7 MB.");
      return;
    }
    const reader = new FileReader();
    reader.onload = () => update("imageUrl", typeof reader.result === "string" ? reader.result : "");
    reader.onerror = () => setMessage("Unable to read image.");
    reader.readAsDataURL(file);
  }
  async function save() { if (!form.name.trim() || !form.category.trim()) { setMessage("Name and category are required."); return; } setSaving(true); try { const result = await apiFetch<Product>(selected ? `/api/products/${selected.id}` : "/api/products", { method: selected ? "PUT" : "POST", body: JSON.stringify(form) }); setItems((current) => selected ? current.map((item) => item.id === result.id ? result : item) : [...current, result]); edit(result); setMessage("Product saved."); } catch (error) { setMessage(error instanceof Error ? error.message : "Unable to save product."); } finally { setSaving(false); } }
  async function remove() { if (!selected || !canDelete || !window.confirm("Delete this product?")) return; await apiFetch(`/api/products/${selected.id}`, { method: "DELETE" }); setItems((current) => current.filter((item) => item.id !== selected.id)); setSelected(null); setForm(empty); }
  if (!canView) return <main className="page"><h1>Products</h1><p>You do not have permission to view products.</p></main>;
  return <main className="page cms-page"><div className="page-header"><div><p className="eyebrow">SUPPLEMENTS & PRODUCTS</p><h1>Products</h1><p className="muted">Manage creatine, fat burners, protein and other gym products.</p></div></div>{message ? <p className="info">{message}</p> : null}<div className="cms-layout"><aside className="card cms-list"><button className="btn" disabled={!canCreate} onClick={() => { setSelected(null); setForm(empty); }}>New product</button>{items.map((item) => <button className={`cms-list-item${selected?.id === item.id ? " selected" : ""}`} key={item.id} onClick={() => edit(item)}><span>{item.name}</span><small>{item.isActive ? "Active" : "Inactive"}</small></button>)}</aside><section className="card cms-form"><div className="form-grid"><div className="form-field"><label>Name<input value={form.name} placeholder="Creatine Monohydrate" onChange={(event) => update("name", event.target.value)} /></label></div><div className="form-field"><label>Category<input value={form.category} placeholder="Supplements" onChange={(event) => update("category", event.target.value)} /></label></div></div><div className="form-field"><label>Description<textarea rows={4} value={form.description} onChange={(event) => update("description", event.target.value)} /></label></div><div className="form-grid"><div className="form-field"><label>Price<input type="number" min="0" value={form.price} onChange={(event) => update("price", Number(event.target.value))} /></label></div><div className="form-field"><label>Display order<input type="number" min="0" value={form.displayOrder} onChange={(event) => update("displayOrder", Number(event.target.value))} /></label></div></div><div className="form-field"><label>Product image</label><div className="image-url-row"><input value={form.imageUrl} onChange={(event) => update("imageUrl", event.target.value)} placeholder="Paste image URL or upload" /><label className="btn btn-secondary upload-button"><input type="file" accept="image/*" onChange={upload} />Upload image</label></div></div>{form.imageUrl ? <img className="cms-preview-image product-admin-preview" src={form.imageUrl} alt={`${form.name || "Product"} preview`} /> : null}<label className="toggle-row"><input type="checkbox" checked={form.isActive} onChange={(event) => update("isActive", event.target.checked)} /> Active</label><div className="cms-form-footer"><button className="btn" disabled={saving || (selected ? !canEdit : !canCreate)} onClick={() => void save()}>{saving ? "Saving…" : selected ? "Save changes" : "Create product"}</button>{selected && canDelete ? <button className="btn btn-danger" onClick={() => void remove()}>Delete</button> : null}</div></section></div></main>;
}
