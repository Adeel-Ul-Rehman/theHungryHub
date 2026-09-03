import React from 'react';
import { Bell, Sparkles, ShieldCheck, RefreshCw } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const Header = ({ title = "Dashboard Overview" }) => {
  const { admin } = useAuth();

  return (
    <header className="sticky top-0 z-20 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md border-b border-slate-200 dark:border-slate-800 px-8 py-4 flex items-center justify-between transition-colors">
      <div>
        <h2 className="text-xl font-extrabold text-slate-900 dark:text-white tracking-tight">{title}</h2>
        <p className="text-xs text-slate-500 dark:text-slate-400 font-medium">Real-Time Operations & Business Affairs Portal</p>
      </div>

      <div className="flex items-center gap-4">
        {/* Live Status Badge */}
        <div className="flex items-center gap-2 px-3 py-1.5 rounded-full bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800/50 text-emerald-600 dark:text-emerald-400 text-xs font-bold">
          <span className="w-2 h-2 rounded-full bg-emerald-500 animate-ping" />
          <span>Live Systems Online</span>
        </div>

        {/* Notifications Icon */}
        <button className="relative p-2 rounded-xl text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white bg-slate-100 dark:bg-slate-800 transition-colors">
          <Bell className="w-5 h-5" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-orange-500 rounded-full" />
        </button>

        {/* Admin Badge */}
        <div className="flex items-center gap-2.5 pl-3 border-l border-slate-200 dark:border-slate-800">
          <div className="w-9 h-9 rounded-full bg-gradient-to-tr from-orange-500 to-amber-400 text-white flex items-center justify-center font-bold text-sm shadow-md">
            {admin.fullName ? admin.fullName[0] : 'A'}
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;
