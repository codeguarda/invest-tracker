import { useEffect, useState } from "react";
import { api } from "../../lib/axios";

type Investment = { id:string; type:string; amount:number; date:string; description?:string; };

export default function InvestmentsPage(){
  const [items,setItems]=useState<Investment[]>([]);
  const [form,setForm]=useState({type:"Ação",amount:0,date:"",description:""});

  async function load(){ const {data}=await api.get("/api/investments?page=1&size=50"); setItems(data); }
  useEffect(()=>{ load(); },[]);

  async function create(e:any){ e.preventDefault();
    await api.post("/api/investments",{
      type: form.type, amount: Number(form.amount), date: form.date, description: form.description||null
    });
    setForm({type:"Ação",amount:0,date:"",description:""}); load();
  }

  return (
    <div style={{padding:24}}>
      <h1>Investimentos</h1>
      <form onSubmit={create} style={{display:"grid",gridTemplateColumns:"1fr 1fr 1fr 2fr auto",gap:8,maxWidth:900}}>
        <input value={form.type} onChange={e=>setForm({...form,type:e.target.value})} placeholder="Tipo"/>
        <input type="number" value={form.amount} onChange={e=>setForm({...form,amount:Number(e.target.value)})} placeholder="Valor"/>
        <input type="date" value={form.date} onChange={e=>setForm({...form,date:e.target.value})} />
        <input placeholder="Descrição" value={form.description} onChange={e=>setForm({...form,description:e.target.value})}/>
        <button>Adicionar</button>
      </form>

      <table style={{marginTop:24,width:"100%",borderCollapse:"collapse"}}>
        <thead><tr><th>Tipo</th><th>Valor</th><th>Data</th><th>Descrição</th></tr></thead>
        <tbody>
          {items.map(i=>(
            <tr key={i.id} style={{borderTop:"1px solid #ddd"}}>
              <td>{i.type}</td>
              <td>{i.amount.toFixed(2)}</td>
              <td>{i.date}</td>
              <td>{i.description}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
