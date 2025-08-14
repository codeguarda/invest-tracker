import { useState } from "react";
import { api } from "../../lib/axios";
import { saveToken } from "../../lib/auth";

export default function LoginPage(){
  const [email,setEmail]=useState("demo@local");
  const [password,setPassword]=useState("demo123");
  const [err,setErr]=useState("");

  async function submit(e:any){ e.preventDefault(); setErr("");
    try{
      const {data} = await api.post("/api/auth/login",{email,password});
      saveToken(data.token); location.href="/dashboard";
    }catch{ setErr("Credenciais inválidas"); }
  }
  return (
    <main style={{minHeight:"100vh",display:"flex",alignItems:"center",justifyContent:"center"}}>
      <form onSubmit={submit} style={{width:320,display:"grid",gap:8}}>
        <h1>Entrar</h1>
        <input placeholder="Email" value={email} onChange={e=>setEmail(e.target.value)} />
        <input placeholder="Senha" type="password" value={password} onChange={e=>setPassword(e.target.value)} />
        {err && <p style={{color:"#b00"}}>{err}</p>}
        <button>Entrar</button>
      </form>
    </main>
  );
}
