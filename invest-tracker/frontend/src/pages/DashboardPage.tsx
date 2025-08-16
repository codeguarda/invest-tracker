import { useEffect, useMemo, useState } from "react";
import { api } from "../lib/axios";
import { LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, BarChart, Bar } from "recharts";
import TopNav from "../components/TopNav";

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
    <>
      <TopNav/>
      <div className="page">
        <h1>Dashboard</h1>

        <div className="card scroll-x">
          <LineChart width={800} height={300} data={lineData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="month"/><YAxis/><Tooltip/>
            <Line type="monotone" dataKey="total" />
          </LineChart>
        </div>

        <div className="card scroll-x">
          <BarChart width={800} height={300} data={barData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="type"/><YAxis/><Tooltip/>
            <Bar dataKey="total" />
          </BarChart>
        </div>
      </div>
    </>
  );
}
