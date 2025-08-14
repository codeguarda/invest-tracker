import { useEffect, useMemo, useState } from "react";
import { api } from "../../lib/axios";
import { LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, BarChart, Bar } from "recharts";
import { Link } from "react-router-dom";

type Item = { month:string; total:number; byType:Record<string,number>; };

export default function DashboardPage(){
  const [data,setData]=useState<Item[]>([]);
  useEffect(()=>{ (async()=>{
    const {data} = await api.get("/api/dashboard");
    setData(data);
  })(); },[]);

  const lineData = useMemo(()=> data.map(d=>({ month: d.month, total: d.total })), [data]);
  const last = data[data.length-1];
  const barData = useMemo(()=> last ? Object.entries(last.byType).map(([k,v])=>({ type:k, total:v })) : [], [last]);

  return (
    <div style={{padding:24}}>
      <h1>Dashboard</h1>
      <p><Link to="/investments">Ir para Investimentos</Link></p>

      <div style={{overflowX:"auto"}}>
        <LineChart width={800} height={300} data={lineData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="month"/><YAxis/><Tooltip/>
          <Line type="monotone" dataKey="total" />
        </LineChart>
      </div>

      <div style={{marginTop:24, overflowX:"auto"}}>
        <BarChart width={800} height={300} data={barData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="type"/><YAxis/><Tooltip/>
          <Bar dataKey="total" />
        </BarChart>
      </div>
    </div>
  );
}
