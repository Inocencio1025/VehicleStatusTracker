import { useState } from "react";
import { useNavigate } from "react-router-dom";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default function Login() {
  const [username, setName] = useState("");
  const [password, setPassword] = useState("");
  const [email, setEmail] = useState("");
  const [isRegister, setIsRegister] = useState(false);
  const [inValid, setInvalid] = useState(false);
  const [userCreated, setUserCreated] = useState(false);
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    if (isRegister) {
      registerUser();
    } else {
      loginUser();
    }
  }

  async function registerUser() {
    const response = await fetch(`${API_BASE_URL}/api/vehicle/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        email: email,
        username: username,
        password: password
      })
    })
    if (response.ok) {
      setIsRegister(false);
      setInvalid(false);
      setUserCreated(true);
    }
  }

  async function loginUser() {
    const response = await fetch(`${API_BASE_URL}/api/vehicle/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        username: username,
        password: password
      })
    })


    if (response.ok) {
      localStorage.setItem("token", response.json.toString());
      navigate("/dashboard");
    } else {
      setInvalid(true);
      setUserCreated(false);
    }

  }

  return (
    <div className="min-h-screen flex flex-col items-center justify-center">
      <div className="p-8 bg-slate-800 rounded w-96 flex flex-col gap-4">

        <div>
          <h1 className="text-center font-bold">
            {isRegister ? "New User" : "Sign In"}
          </h1>
          <h4 className="text-center text-yellow-500">
            {isRegister && "reminder: create validation for these field you potato"}
          </h4>
        </div>

        {(inValid && !isRegister) && (
          <h3 className="text-red-500 text-center">
            Invalid Username or Password
          </h3>
        )}
                
        {(userCreated && !isRegister) && (
          <h3 className="text-green-500 text-center">
            User Created
          </h3>
        )}

        <form onSubmit={handleSubmit}>
          <div className="flex flex-col my-5 gap-4">

            {isRegister && (
              <input
                className="p-2 border rounded"
                type="text"
                placeholder="Email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            )}

            <input
              className="p-2 border rounded"
              type="text"
              placeholder="Username"
              value={username}
              onChange={(e) => setName(e.target.value)}
            />

            <input
              className="p-2 border rounded"
              type="password"
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>


          <div className="flex flex-col gap-2">
            <button
              className="bg-emerald-500 rounded text-xl p-2">
              {isRegister ? "Create Account" : "Sign In"}
            </button>
            <button
              type="button"
              className="bg-emerald-800 rounded text-xl p-2"
              onClick={() => setIsRegister(!isRegister)}
            >
              {isRegister ? "Login (Existing User)" : "Register (New User)"}
            </button>
          </div>
        </form>

      </div>
    </div>
  );
}