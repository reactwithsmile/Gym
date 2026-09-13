import { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { apiFetch, apiUrl, getToken } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { Permission } from "../types/auth";

type Notification = { id: number; title: string; message: string; type: string; isRead: boolean; createdAt: string };

export function NotificationsBell() {
  const { hasPermission } = useAuth();
  const [items, setItems] = useState<Notification[]>([]);
  const [open, setOpen] = useState(false);
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
  if (!canView) return null;
  const unread = items.filter((item) => !item.isRead).length;
  async function markRead(id: number) { await apiFetch(`/api/notifications/${id}/read`, { method: "PUT" }); setItems((current) => current.map((item) => item.id === id ? { ...item, isRead: true } : item)); }
  async function markAllRead() { await apiFetch("/api/notifications/read-all", { method: "PUT" }); setItems((current) => current.map((item) => ({ ...item, isRead: true }))); }
  return <div className="notification-area"><button className="notification-bell" type="button" onClick={() => setOpen((value) => !value)} aria-label="Notifications"><span aria-hidden="true">♢</span>{unread ? <b>{unread}</b> : null}</button>{open ? <div className="notification-popover"><div className="notification-header"><strong>Notifications</strong>{unread ? <button className="btn-link" onClick={() => void markAllRead()}>Mark all read</button> : null}</div>{items.length ? items.slice(0, 8).map((item) => <button className={`notification-item${item.isRead ? "" : " unread"}`} key={item.id} onClick={() => void markRead(item.id)}><strong>{item.title}</strong><span>{item.message}</span><small>{new Date(item.createdAt).toLocaleString()}</small></button>) : <p className="muted">No notifications.</p>}</div> : null}</div>;
}
