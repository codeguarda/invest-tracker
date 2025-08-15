import { useEffect, useState } from "react";
import { api } from "../../lib/axios";

type Investment = {
  id: string;
  type: string;
  amount: number;
  date: string;           // ISO vindo da API (ex.: 2025-08-15T00:00:00)
  description?: string;
};

// helpers de formatação
function parseDateSafe(s?: string): Date | null {
  if (!s) return null;
  // aceita "YYYY-MM-DD" ou ISO completo
  const onlyDate = /^\d{4}-\d{2}-\d{2}$/.test(s);
  const d = new Date(onlyDate ? `${s}T00:00:00` : s);
  return isNaN(d.getTime()) ? null : d;
}
function fmtLocalDate(s?: string) {
  const d = parseDateSafe(s);
  return d ? d.toLocaleDateString() : "";
}
function fmtBRL(n: number) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(n);
}

export default function InvestmentsPage() {
  const [items, setItems] = useState<Investment[]>([]);
  const [form, setForm] = useState({ type: "Ação", amount: 0, date: "", description: "" });

  async function load() {
    const { data } = await api.get("/api/investments?page=1&size=50");
    setItems(data);
  }

  useEffect(() => {
    // sugere a data de hoje no input
    const today = new Date();
    const pad = (x: number) => String(x).padStart(2, "0");
    const yyyy = today.getFullYear();
    const mm = pad(today.getMonth() + 1);
    const dd = pad(today.getDate());
    setForm((f) => ({ ...f, date: `${yyyy}-${mm}-${dd}` }));
    load();
  }, []);

  async function create(e: React.FormEvent) {
    e.preventDefault();
    await api.post("/api/investments", {
      type: form.type,
      amount: Number(form.amount),
      date: form.date, // já está em YYYY-MM-DD
      description: form.description || null,
    });
    setForm({ type: "Ação", amount: 0, date: form.date, description: "" });
    load();
  }

  return (
    <div style={{ padding: 24 }}>
      <h1>Investimentos</h1>

      <form
        onSubmit={create}
        style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 2fr auto", gap: 8, maxWidth: 900 }}
      >
        <input
          value={form.type}
          onChange={(e) => setForm({ ...form, type: e.target.value })}
          placeholder="Tipo"
        />
        <input
          type="number"
          value={form.amount}
          onChange={(e) => setForm({ ...form, amount: Number(e.target.value) })}
          placeholder="Valor"
        />
        <input
          type="date"
          value={form.date}
          onChange={(e) => setForm({ ...form, date: e.target.value })}
        />
        <input
          placeholder="Descrição"
          value={form.description}
          onChange={(e) => setForm({ ...form, description: e.target.value })}
        />
        <button>Adicionar</button>
      </form>

      <table style={{ marginTop: 24, width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            <th style={{ textAlign: "left", padding: "8px 0" }}>Tipo</th>
            <th style={{ textAlign: "right", padding: "8px 0" }}>Valor</th>
            <th style={{ textAlign: "center", padding: "8px 0" }}>Data</th>
            <th style={{ textAlign: "left", padding: "8px 0" }}>Descrição</th>
          </tr>
        </thead>
        <tbody>
          {items.map((i) => (
            <tr key={i.id} style={{ borderTop: "1px solid #ddd" }}>
              <td style={{ padding: "8px 0" }}>{i.type}</td>
              <td style={{ padding: "8px 0", textAlign: "right" }}>{fmtBRL(i.amount)}</td>
              <td style={{ padding: "8px 0", textAlign: "center" }}>{fmtLocalDate(i.date)}</td>
              <td style={{ padding: "8px 0" }}>{i.description}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

