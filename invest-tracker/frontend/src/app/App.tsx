import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import LoginPage from "../features/auth/LoginPage";
import InvestmentsPage from "../features/investments/InvestmentsPage";
import DashboardPage from "../features/dashboard/DashboardPage";

const Private = ({children}:{children:JSX.Element}) =>
  (localStorage.getItem("token") ? children : <Navigate to="/login" />);

export default function App(){
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage/>}/>
        <Route path="/investments" element={<Private><InvestmentsPage/></Private>}/>
        <Route path="/dashboard" element={<Private><DashboardPage/></Private>}/>
        <Route path="*" element={<Navigate to="/dashboard" />}/>
      </Routes>
    </BrowserRouter>
  );
}
