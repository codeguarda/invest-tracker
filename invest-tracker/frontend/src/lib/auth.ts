export function saveToken(t: string){ localStorage.setItem("token", t); }
export function logout(){ localStorage.removeItem("token"); location.href="/login"; }
export function isAuthed(){ return !!localStorage.getItem("token"); }
