import { useEffect, useState } from "react";
import { api } from "../lib/axios";
import TopNav from "../components/TopNav";

type Investment = { id:string; type:string; amount:number; date:string; description?:string; };

export default function InvestmentsPage(){
  const [items,setItems]=useState<Investment[]>([]);
  const [form,setForm]=useState({type:"Ação",amount:0,date:"",description:""});
  const [loading,setLoading]=useState(false);

  async function load(){
    const {data}=await api.get("/api/investments?page=1&size=100");
    setItems(data);
  }
  useEffect(()=>{ load(); },[]);

  async function create(e:any){
    e.preventDefault();
    setLoading(true);
    await api.post("/api/investments",{
      type: form.type,
      amount: Number(form.amount),
      date: form.date,
      description: form.description || null
    });
    setForm({type:"Ação",amount:0,date:"",description:""});
    await load();
    setLoading(false);
  }

  return (
    <>
      <TopNav/>
      <div className="page">
        <h1>Investimentos</h1>

        <form onSubmit={create} className="card form-grid">
          <input value={form.type} onChange={e=>setForm({...form,type:e.target.value})} placeholder="Tipo (ex.: Ação)"/>
          <input type="number" value={form.amount} onChange={e=>setForm({...form,amount:Number(e.target.value)})} placeholder="Valor"/>
          <input type="date" value={form.date} onChange={e=>setForm({...form,date:e.target.value})} />
          <input placeholder="Descrição" value={form.description} onChange={e=>setForm({...form,description:e.target.value})}/>
          <button className="btn primary" disabled={loading}>{loading?"Salvando...":"Adicionar"}</button>
        </form>

        <div className="card">
          <table className="table">
            <thead><tr><th>Tipo</th><th>Valor</th><th>Data</th><th>Descrição</th></tr></thead>
            <tbody>
              {items.map(i=>(
                <tr key={i.id}>
                  <td>{i.type}</td>
                  <td>{i.amount.toFixed(2)}</td>
                  <td>{new Date(i.date).toLocaleDateString()}</td>
                  <td>{i.description}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
