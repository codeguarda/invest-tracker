import { useState } from "react";
import { api } from "../lib/axios";
import { setToken, isAuthed } from "../lib/auth";
import { useNavigate } from "react-router-dom";

export default function AuthPage(){
  const [tab,setTab] = useState<"login"|"register">("login");
  const [email,setEmail] = useState("");
  const [password,setPassword] = useState("");
  const [loading,setLoading] = useState(false);
  const [error,setError] = useState<string|null>(null);
  const nav = useNavigate();

  if (isAuthed()) {
    // já está logado
    nav("/dashboard");
  }

  async function submit(e:React.FormEvent){
    e.preventDefault();
    setError(null);
    setLoading(true);
    try{
      const url = tab==="login" ? "/api/auth/login" : "/api/auth/register";
      const {data} = await api.post(url, { email, password });
      if (!data?.token) throw new Error("Resposta sem token");
      setToken(data.token);
      nav("/dashboard");
    }catch(err:any){
      setError(err?.response?.data?.error || err?.message || "Falha");
    }finally{
      setLoading(false);
    }
  }

  return (
    <div className="auth-wrap">
      <div className="card">
        <div className="tabs-inline">
          <button className={tab==="login"?"tab active":"tab"} onClick={()=>setTab("login")}>Entrar</button>
          <button className={tab==="register"?"tab active":"tab"} onClick={()=>setTab("register")}>Cadastrar</button>
        </div>

        <form onSubmit={submit} className="form">
          <label>Email</label>
          <input type="email" required value={email} onChange={e=>setEmail(e.target.value)} placeholder="voce@exemplo.com" />

          <label>Senha</label>
          <input type="password" required value={password} onChange={e=>setPassword(e.target.value)} placeholder="••••••••" />

          {error && <div className="error">{error}</div>}

          <button className="btn primary" disabled={loading}>
            {loading ? "Enviando..." : (tab==="login" ? "Entrar" : "Criar conta")}
          </button>
        </form>
      </div>
    </div>
  );
}
