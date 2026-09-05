type PlaceholderPageProps = {
  title: string;
};

export function PlaceholderPage({ title }: PlaceholderPageProps) {
  return (
    <main className="page">
      <h1>{title}</h1>
      <p>This module is not implemented yet.</p>
    </main>
  );
}
