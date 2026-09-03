import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Boxes, 
  ShoppingBag, 
  UtensilsCrossed, 
  Bot, 
  UserCheck, 
  Sun, 
  Moon, 
  LogOut,
  Flame
} from 'lucide-react';
import { useTheme } from '../context/ThemeContext';
import { useAuth } from '../context/AuthContext';

const Sidebar = () => {
  const { isDarkMode, toggleTheme } = useTheme();
  const { logout, admin } = useAuth();
  const navigate = useNavigate();

  const navItems = [
    { label: 'Dashboard', icon: LayoutDashboard, path: '/' },
    { label: 'Inventory Stock', icon: Boxes, path: '/inventory' },
    { label: 'Sales & Orders', icon: ShoppingBag, path: '/orders' },
    { label: 'Live Menu', icon: UtensilsCrossed, path: '/menu' },
    { label: 'HuggingFace AI', icon: Bot, path: '/ai-assistant', badge: 'AI' },
    { label: 'Admin Profile', icon: UserCheck, path: '/profile' },
  ];

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside className="w-64 bg-white dark:bg-slate-900 border-r border-slate-200 dark:border-slate-800 flex flex-col justify-between h-screen sticky top-0 z-30 transition-colors">
      <div>
        {/* Brand Header */}
        <div className="p-6 flex items-center gap-3 border-b border-slate-100 dark:border-slate-800/80">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-amber-500 to-orange-600 flex items-center justify-center text-white shadow-lg shadow-orange-500/20">
            <Flame className="w-6 h-6 fill-white" />
          </div>
          <div>
            <h1 className="font-extrabold text-lg text-slate-900 dark:text-white leading-tight">HUNGRY HUB</h1>
            <p className="text-xs font-semibold text-orange-600 dark:text-orange-400">Web Admin & AI Control</p>
          </div>
        </div>

        {/* Navigation Links */}
        <nav className="p-4 space-y-1.5">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.path}
                to={item.path}
                className={({ isActive }) =>
                  `flex items-center justify-between px-4 py-3 rounded-xl font-semibold text-sm transition-all duration-150 ${
                    isActive
                      ? 'bg-gradient-to-r from-orange-500 to-amber-500 text-white shadow-md shadow-orange-500/25'
                      : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800/60 hover:text-slate-900 dark:hover:text-white'
                  }`
                }
              >
                <div className="flex items-center gap-3">
                  <Icon className="w-5 h-5" />
                  <span>{item.label}</span>
                </div>
                {item.badge && (
                  <span className="px-2 py-0.5 text-[10px] font-extrabold bg-amber-400 text-slate-950 rounded-full animate-pulse">
                    {item.badge}
                  </span>
                )}
              </NavLink>
            );
          })}
        </nav>
      </div>

      {/* Bottom Actions */}
      <div className="p-4 border-t border-slate-100 dark:border-slate-800/80 space-y-3">
        {/* Dark/Light Mode Toggle */}
        <button
          onClick={toggleTheme}
          className="w-full flex items-center justify-between px-4 py-2.5 rounded-xl bg-slate-100 dark:bg-slate-800/80 text-slate-700 dark:text-slate-300 font-semibold text-xs hover:opacity-90 transition-opacity"
        >
          <span className="flex items-center gap-2">
            {isDarkMode ? <Moon className="w-4 h-4 text-indigo-400" /> : <Sun className="w-4 h-4 text-amber-500" />}
            <span>{isDarkMode ? 'Dark Mode' : 'Light Mode'}</span>
          </span>
          <span className="text-[10px] uppercase font-bold text-slate-400">{isDarkMode ? 'ON' : 'OFF'}</span>
        </button>

        {/* User Snippet */}
        <div className="flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-slate-800/40">
          <div className="truncate">
            <p className="text-xs font-bold text-slate-900 dark:text-white truncate">{admin.fullName}</p>
            <p className="text-[10px] text-slate-500 dark:text-slate-400 truncate">{admin.email}</p>
          </div>
          <button
            onClick={handleLogout}
            title="Logout"
            className="p-1.5 rounded-lg text-slate-400 hover:text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/30 transition-colors"
          >
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;
