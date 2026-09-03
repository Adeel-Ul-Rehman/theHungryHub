import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Flame, Lock, Mail, ArrowRight, ShieldCheck } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const LoginPage = () => {
  const [email, setEmail] = useState('admin@thehungryhub.shop');
  const [password, setPassword] = useState('admin123');
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');
  
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setErrorMsg('');

    try {
      if (!email || !password) {
        setErrorMsg('Please provide both email and password');
        setLoading(false);
        return;
      }

      await login(email, password);
      navigate('/');
    } catch (err) {
      setErrorMsg('Invalid admin credentials. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden font-['Plus_Jakarta_Sans',sans-serif]">
      {/* Background Ambient Glows */}
      <div className="absolute -top-40 -left-40 w-96 h-96 bg-orange-600/20 rounded-full blur-3xl" />
      <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-amber-600/20 rounded-full blur-3xl" />

      <div className="relative z-10 w-full max-w-md p-8 rounded-3xl bg-slate-900 border border-slate-800/80 shadow-2xl space-y-8">
        {/* Brand Header */}
        <div className="text-center space-y-3">
          <div className="inline-flex w-14 h-14 rounded-2xl bg-gradient-to-tr from-amber-500 to-orange-600 items-center justify-center text-white shadow-xl shadow-orange-500/30">
            <Flame className="w-8 h-8 fill-white" />
          </div>
          <h1 className="text-2xl font-black text-white tracking-tight">HUNGRY HUB</h1>
          <p className="text-xs text-orange-400 font-extrabold uppercase tracking-wider flex items-center justify-center gap-1.5">
            <ShieldCheck className="w-4 h-4 text-orange-400" /> Web Admin JWT Authentication
          </p>
        </div>

        {errorMsg && (
          <div className="p-3.5 rounded-xl bg-rose-950/60 border border-rose-800 text-rose-300 text-xs font-bold text-center">
            {errorMsg}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="text-xs font-bold text-slate-400 uppercase tracking-wider">Admin Email</label>
            <div className="relative mt-1.5">
              <Mail className="w-4 h-4 absolute left-3.5 top-3 text-slate-500" />
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-800 bg-slate-950 text-white text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-orange-500"
              />
            </div>
          </div>

          <div>
            <label className="text-xs font-bold text-slate-400 uppercase tracking-wider">Password</label>
            <div className="relative mt-1.5">
              <Lock className="w-4 h-4 absolute left-3.5 top-3 text-slate-500" />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-800 bg-slate-950 text-white text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-orange-500"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-3.5 rounded-xl bg-gradient-to-r from-orange-500 to-amber-500 text-white font-extrabold text-sm shadow-xl shadow-orange-500/25 flex items-center justify-center gap-2 hover:opacity-90 transition-opacity"
          >
            {loading ? 'Authenticating...' : 'Sign In to Control Panel'} <ArrowRight className="w-4 h-4" />
          </button>
        </form>

        <p className="text-center text-[11px] text-slate-500">
          Encrypted JWT Session Protection & Real-Time Restaurant Management
        </p>
      </div>
    </div>
  );
};

export default LoginPage;
