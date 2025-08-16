import { NavLink, useNavigate } from "react-router-dom";
import { clearToken } from "../lib/auth";

export default function TopNav(){
  const nav = useNavigate();
  function logout(){
    clearToken();
    nav("/");
  }
  return (
    <div className="topnav">
      <div className="brand">InvestTracker</div>
      <div className="tabs">
        <NavLink to="/dashboard" className={({isActive})=> isActive?"tab active":"tab"}>Dashboard</NavLink>
        <NavLink to="/investments" className={({isActive})=> isActive?"tab active":"tab"}>Investimentos</NavLink>
      </div>
      <button className="btn outline" onClick={logout}>Sair</button>
    </div>
  );
}
