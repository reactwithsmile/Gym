import { useEffect, useMemo, useState } from "react";
import { apiFetch } from "../../api/client";

type ActionKey = "view" | "create" | "edit" | "delete";

type PermissionApiItem = {
  id: number;
  code: string;
  name: string;
};

type RoleApiResponse = {
  id: number;
  name: string;
  permissions: PermissionApiItem[];
  permissionCodes: string[];
};

type PermissionMatrixEntry = {
  module: string;
  key: string;
  actions: ActionKey[];
  codes: Record<ActionKey, string>;
};

const actionLabels: Record<ActionKey, string> = {
  view: "View",
  create: "Create",
  edit: "Edit",
  delete: "Delete",
};

const permissionMatrix: PermissionMatrixEntry[] = [
  { module: "Dashboard", key: "dashboard", actions: ["view"], codes: { view: "dashboard.view", create: "", edit: "", delete: "" } },
  { module: "Hero", key: "hero", actions: ["view", "create", "edit", "delete"], codes: { view: "hero.view", create: "hero.create", edit: "hero.edit", delete: "hero.delete" } },
  { module: "About", key: "about", actions: ["view", "create", "edit", "delete"], codes: { view: "about.view", create: "about.create", edit: "about.edit", delete: "about.delete" } },
  { module: "Services", key: "services", actions: ["view", "create", "edit", "delete"], codes: { view: "services.view", create: "services.create", edit: "services.edit", delete: "services.delete" } },
  { module: "Trainers", key: "trainers", actions: ["view", "create", "edit", "delete"], codes: { view: "trainers.view", create: "trainers.create", edit: "trainers.edit", delete: "trainers.delete" } },
  { module: "Membership Plans", key: "membership", actions: ["view", "create", "edit", "delete"], codes: { view: "membership.view", create: "membership.create", edit: "membership.edit", delete: "membership.delete" } },
  { module: "Gallery", key: "gallery", actions: ["view", "create", "edit", "delete"], codes: { view: "gallery.view", create: "gallery.create", edit: "gallery.edit", delete: "gallery.delete" } },
  { module: "Testimonials", key: "testimonials", actions: ["view", "create", "edit", "delete"], codes: { view: "testimonials.view", create: "testimonials.create", edit: "testimonials.edit", delete: "testimonials.delete" } },
  { module: "Contact", key: "contact", actions: ["view", "edit"], codes: { view: "contact.view", create: "", edit: "contact.edit", delete: "" } },
  { module: "Settings", key: "settings", actions: ["view", "edit"], codes: { view: "settings.view", create: "", edit: "settings.edit", delete: "" } },
  { module: "Roles & Permissions", key: "roles", actions: ["view", "edit"], codes: { view: "roles.view", create: "", edit: "roles.edit", delete: "" } },
  { module: "Enquiries", key: "enquiries", actions: ["view", "edit"], codes: { view: "enquiries.view", create: "", edit: "enquiries.edit", delete: "" } },
  { module: "Products", key: "products", actions: ["view", "create", "edit", "delete"], codes: { view: "products.view", create: "products.create", edit: "products.edit", delete: "products.delete" } },
];

const tableHeaderActions: ActionKey[] = ["view", "create", "edit", "delete"];

export function RolesPage() {
  const [roles, setRoles] = useState<RoleApiResponse[]>([]);
  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(null);
  const [permissionDraft, setPermissionDraft] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

  async function loadRoles() {
    setLoading(true);
    try {
      const data = await apiFetch<RoleApiResponse[]>("/api/admin/roles");
      const sorted = (data ?? []).slice().sort((a, b) => {
        if (a.name === "Admin") return -1;
        if (b.name === "Admin") return 1;
        return a.name.localeCompare(b.name);
      });

      setRoles(sorted);
      if (sorted.length > 0) {
        setSelectedRoleId((current) => {
          if (current && sorted.some((role) => role.id === current)) {
            return current;
          }
          const preferredAdmin = sorted.find((role) => role.name === "Admin");
          return preferredAdmin?.id ?? sorted[0].id;
        });
      } else {
        setSelectedRoleId(null);
      }
    } catch (error) {
      setRoles([]);
      setSelectedRoleId(null);
      setMessage({
        type: "error",
        text: error instanceof Error ? error.message : "Unable to load roles.",
      });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadRoles();
  }, []);

  const selectedRole = useMemo(
    () => roles.find((role) => role.id === selectedRoleId) ?? null,
    [roles, selectedRoleId],
  );

  useEffect(() => {
    if (!selectedRole) {
      setPermissionDraft([]);
      return;
    }

    setPermissionDraft((selectedRole.permissionCodes ?? []).slice().sort((a, b) => a.localeCompare(b)));
    setMessage(null);
  }, [selectedRole]);

  function togglePermission(code: string, enabled: boolean) {
    setPermissionDraft((current) => {
      const next = new Set(current);
      if (enabled) {
        next.add(code);
      } else {
        next.delete(code);
      }
      return Array.from(next).sort((a, b) => a.localeCompare(b));
    });
  }

  async function onSave() {
    if (!selectedRole) {
      return;
    }

    setSaving(true);
    setMessage(null);

    try {
      const updatedRole = await apiFetch<RoleApiResponse>(`/api/admin/roles/${selectedRole.id}/permissions`, {
        method: "PUT",
        body: JSON.stringify({ permissionCodes: permissionDraft }),
      });

      setRoles((current) => current.map((role) => (role.id === updatedRole.id ? updatedRole : role)));
      setPermissionDraft((updatedRole.permissionCodes ?? []).slice().sort((a, b) => a.localeCompare(b)));
      setMessage({ type: "success", text: "Permissions saved successfully." });
    } catch (error) {
      setMessage({
        type: "error",
        text: error instanceof Error ? error.message : "Unable to save permissions.",
      });
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <main className="page">
        <h1>Roles & Permissions</h1>
        <p>Loading…</p>
      </main>
    );
  }

  if (!roles.length) {
    return (
      <main className="page">
        <h1>Roles & Permissions</h1>
        <p>No roles available.</p>
      </main>
    );
  }

  return (
    <main className="page roles-page">
      <div className="roles-header">
        <h1>Roles & Permissions</h1>
        <p className="muted">Manage role access across the admin dashboard.</p>
      </div>

      <div className="roles-grid">
        <aside className="roles-panel">
          <label className="field">
            Role
            <select
              value={selectedRoleId ?? ""}
              onChange={(event) => setSelectedRoleId(Number(event.target.value))}
            >
              {roles.map((role) => (
                <option key={role.id} value={role.id}>
                  {role.name}
                </option>
              ))}
            </select>
          </label>

          <div className="card">
            <h3 className="card-title">Selected Role</h3>
            <p className="card-body">
              {selectedRole ? `${selectedRole.name} — ${selectedRole.permissionCodes.length} permissions` : "No role selected"}
            </p>
          </div>

          <button className="btn btn-primary" onClick={onSave} disabled={saving || !selectedRole}>
            {saving ? "Saving…" : "Save Changes"}
          </button>

          {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}
        </aside>

        <section className="roles-content">
          <div className="roles-card card">
            <table className="roles-table" role="table">
              <thead>
                <tr>
                  <th>Permission</th>
                  {tableHeaderActions.map((action) => (
                    <th key={action}>{actionLabels[action]}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {permissionMatrix.map((entry) => (
                  <tr key={entry.key} className="perm-row">
                    <td className="perm-name" data-label="Permission">
                      {entry.module}
                    </td>
                    {tableHeaderActions.map((action) => {
                      const actionCode = entry.codes[action];
                      const isVisible = entry.actions.includes(action) && actionCode;

                      if (!isVisible) {
                        return <td key={`${entry.key}-${action}`} data-label={actionLabels[action]} />;
                      }

                      return (
                        <td key={`${entry.key}-${action}`} data-label={actionLabels[action]}>
                          <input
                            type="checkbox"
                            checked={permissionDraft.includes(actionCode)}
                            onChange={(event) => togglePermission(actionCode, event.target.checked)}
                          />
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </main>
  );
}
