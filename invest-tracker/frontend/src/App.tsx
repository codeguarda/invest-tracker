import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import AuthPage from "./pages/AuthPage";
import DashboardPage from "./pages/DashboardPage";
import InvestmentsPage from "./pages/InvestmentsPage";
import { isAuthed } from "./lib/auth";
import "./styles.css";

function RequireAuth({ children }: { children: JSX.Element }) {
  return isAuthed() ? children : <Navigate to="/" replace />;
}

export default function App(){
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<AuthPage/>} />
        <Route path="/dashboard" element={<RequireAuth><DashboardPage/></RequireAuth>} />
        <Route path="/investments" element={<RequireAuth><InvestmentsPage/></RequireAuth>} />
        <Route path="*" element={<Navigate to="/" replace/>} />
      </Routes>
    </BrowserRouter>
  );
}
