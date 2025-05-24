/*import { BrowserRouter as Router, Route, Routes} from "react-router-dom"
import { Home, Leads, Login, Profiles, Registration } from "./pages"

function App() {
 

  return (
   <Router>
      <Routes>
        <Route path="/" element={<Login/>} />
        <Route path="/cadastro" element={<Registration/>} />
        <Route path="/home" element={<Home/>} />
        <Route path="/leads" element={<Leads/>} />
        <Route path="/perfil" element={<Profiles/>} />
      </Routes>
   </Router>
  )
}

export default App
*/

import { BrowserRouter as Router, Route, Routes, Navigate, Outlet } from "react-router-dom"
import Cookies from "js-cookie"
import { Home, Leads, Login, Profiles, Registration } from "./pages"

function App() {
  const ProtecteRoute = () => {
    const checkAuthCookies = Cookies.get('Authorization') 
    if (! checkAuthCookies) {
      alert('Autenticaçao necessaria')
      return <Navigate to="/" replace />
    }
    return <Outlet />;
  }

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/cadastro" element={<Registration />} />
        <Route element={<ProtecteRoute />}>
            <Route path="/home" element={<Home />} />
            <Route path="/leads" element={<Leads />} />
            <Route path="/perfil" element={<Profiles />} />
        </Route>
    
      </Routes>
    </Router>
  )
}

export default App
