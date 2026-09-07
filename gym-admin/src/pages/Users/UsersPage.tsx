import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type User = { id: number; name: string; email: string; roleId: number; roleName: string; isActive: boolean; createdAt: string; updatedAt: string };
type Role = { id: number; name: string };
type Form = { name: string; email: string; password: string; roleId: number; isActive: boolean };
const emptyForm: Form = { name: "", email: "", password: "", roleId: 0, isActive: true };

export function UsersPage() {
  const { hasPermission } = useAuth();
  const [users, setUsers] = useState<User[]>([]); const [roles, setRoles] = useState<Role[]>([]);
  const [form, setForm] = useState<Form>(emptyForm); const [editingId, setEditingId] = useState<number | null>(null);
  const [loading, setLoading] = useState(true); const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canCreate = hasPermission(Permission.UsersCreate); const canEdit = hasPermission(Permission.UsersEdit); const canDelete = hasPermission(Permission.UsersDelete);

  async function load() {
    setLoading(true);
    try { const [userData, roleData] = await Promise.all([apiFetch<User[]>("/api/admin/users"), apiFetch<Role[]>("/api/admin/roles")]); setUsers(userData); setRoles(roleData); if (!form.roleId && roleData[0]) setForm((current) => ({ ...current, roleId: roleData[0].id })); }
    catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load users." }); }
    finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);
  function update<K extends keyof Form>(key: K, value: Form[K]) { setForm((current) => ({ ...current, [key]: value })); }
  function edit(user: User) { setEditingId(user.id); setForm({ name: user.name, email: user.email, password: "", roleId: user.roleId, isActive: user.isActive }); setMessage(null); }
  function reset() { setEditingId(null); setForm({ ...emptyForm, roleId: roles[0]?.id ?? 0 }); }
  async function save() {
    if (!form.name.trim() || !form.email.trim() || !form.roleId || (!editingId && !form.password)) { setMessage({ type: "error", text: "Name, email, role and password are required for a new user." }); return; }
    if (!editingId && form.password.length < 8) { setMessage({ type: "error", text: "Password must be at least 8 characters." }); return; }
    setSaving(true);
    try { const payload = editingId ? { ...form, password: form.password || undefined } : form; const saved = await apiFetch<User>(editingId ? `/api/admin/users/${editingId}` : "/api/admin/users", { method: editingId ? "PUT" : "POST", body: JSON.stringify(payload) }); setUsers((current) => editingId ? current.map((item) => item.id === saved.id ? saved : item) : [...current, saved]); setMessage({ type: "success", text: editingId ? "User updated successfully." : "User created successfully." }); reset(); }
    catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save user." }); }
    finally { setSaving(false); }
  }
  async function deactivate(user: User) {
    if (!canDelete || !window.confirm(`Deactivate ${user.name || user.email}?`)) return;
    try { await apiFetch<void>(`/api/admin/users/${user.id}`, { method: "DELETE" }); setUsers((current) => current.map((item) => item.id === user.id ? { ...item, isActive: false } : item)); setMessage({ type: "success", text: "User deactivated." }); }
    catch (error) { setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to deactivate user." }); }
  }
  if (!hasPermission(Permission.UsersView)) return <main className="page"><h1>Users</h1><p>You do not have permission to view this page.</p></main>;
  return <main className="page cms-page"><div className="page-header"><div><h1>Users</h1><p className="muted">Manage admin access without exposing passwords.</p></div>{canCreate ? <button className="btn" onClick={reset}>Add User</button> : null}</div>{message ? <p className={`info ${message.type}`}>{message.text}</p> : null}{(editingId || canCreate) && <section className="card cms-form"><h2>{editingId ? "Edit User" : "Add User"}</h2><div className="form-grid"><div className="form-field"><label>Name<input value={form.name} onChange={(event) => update("name", event.target.value)} /></label></div><div className="form-field"><label>Email<input type="email" value={form.email} onChange={(event) => update("email", event.target.value)} /></label></div><div className="form-field"><label>Role<select value={form.roleId} onChange={(event) => update("roleId", Number(event.target.value))}><option value={0}>Select role</option>{roles.map((role) => <option key={role.id} value={role.id}>{role.name}</option>)}</select></label></div><div className="form-field"><label>{editingId ? "New Password (optional)" : "Password"}<input type="password" value={form.password} onChange={(event) => update("password", event.target.value)} /></label></div></div><label className="checkbox-field"><input type="checkbox" checked={form.isActive} onChange={(event) => update("isActive", event.target.checked)} /> Active</label><div className="cms-form-footer cms-form-footer-end"><button className="btn secondary" onClick={reset}>Cancel</button><button className="btn" onClick={save} disabled={saving || (editingId ? !canEdit : !canCreate)}>{saving ? "Saving…" : "Save User"}</button></div></section>}{loading ? <p>Loading users…</p> : <section className="card table-wrap"><table><thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th>Created</th><th>Actions</th></tr></thead><tbody>{users.map((user) => <tr key={user.id}><td>{user.name || "—"}</td><td>{user.email}</td><td>{user.roleName}</td><td>{user.isActive ? "Active" : "Inactive"}</td><td>{new Date(user.createdAt).toLocaleDateString()}</td><td>{canEdit ? <button className="btn-link" onClick={() => edit(user)}>Edit</button> : null}{canDelete && user.isActive ? <button className="btn-link danger" onClick={() => void deactivate(user)}>Deactivate</button> : null}</td></tr>)}</tbody></table></section>}</main>;
}
