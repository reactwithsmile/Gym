import { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { apiFetch, apiUrl, getToken } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { Permission } from "../types/auth";

type Notification = { id: number; title: string; message: string; type: string; isRead: boolean; createdAt: string };

export function NotificationsBell() {
  const { hasPermission } = useAuth();
  const [items, setItems] = useState<Notification[]>([]);
  const [open, setOpen] = useState(false);
  const areaRef = useRef<HTMLDivElement>(null);
  const canView = hasPermission(Permission.NotificationsView);
  useEffect(() => {
    if (!canView) return;
    const load = () => void apiFetch<Notification[]>("/api/notifications").then(setItems).catch(() => undefined);
    load();
    const connection = new HubConnectionBuilder().withUrl(apiUrl("/hubs/notifications"), { accessTokenFactory: () => getToken() ?? "" }).withAutomaticReconnect().configureLogging(LogLevel.Warning).build();
    connection.on("NotificationCreated", load);
    connection.on("MembershipExpiring", load);
    connection.on("FeeUpdated", load);
    void connection.start().catch(() => undefined);
    return () => { connection.off("NotificationCreated", load); connection.off("MembershipExpiring", load); connection.off("FeeUpdated", load); void connection.stop(); };
  }, [canView]);
  useEffect(() => {
    if (!open) return;
    function closeOnOutsideClick(event: MouseEvent) {
      if (areaRef.current && !areaRef.current.contains(event.target as Node)) setOpen(false);
    }
    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", closeOnOutsideClick);
    document.addEventListener("keydown", closeOnEscape);
    return () => {
      document.removeEventListener("mousedown", closeOnOutsideClick);
      document.removeEventListener("keydown", closeOnEscape);
    };
  }, [open]);
  if (!canView) return null;
  const unread = items.filter((item) => !item.isRead).length;
  async function markRead(id: number) { await apiFetch(`/api/notifications/${id}/read`, { method: "PUT" }); setItems((current) => current.map((item) => item.id === id ? { ...item, isRead: true } : item)); }
  async function markAllRead() { await apiFetch("/api/notifications/read-all", { method: "PUT" }); setItems((current) => current.map((item) => ({ ...item, isRead: true }))); }
  return <div className="notification-area" ref={areaRef}><button className="notification-bell" type="button" onClick={() => setOpen((value) => !value)} aria-label="Notifications" aria-expanded={open}><svg aria-hidden="true" viewBox="0 0 24 24" focusable="false"><path d="M18 9a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" /></svg>{unread ? <b>{unread}</b> : null}</button>{open ? <div className="notification-popover"><div className="notification-header"><strong>Notifications</strong>{unread ? <button className="btn-link" onClick={() => void markAllRead()}>Mark all read</button> : null}</div>{items.length ? items.slice(0, 8).map((item) => <button className={`notification-item${item.isRead ? "" : " unread"}`} key={item.id} onClick={() => void markRead(item.id)}><strong>{item.title}</strong><span>{item.message}</span><small>{new Date(item.createdAt).toLocaleString()}</small></button>) : <p className="muted">No notifications.</p>}</div> : null}</div>;
}
