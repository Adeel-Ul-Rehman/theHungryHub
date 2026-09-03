import React, { useState, useRef, useEffect } from 'react';
import { Bell, Sparkles, X, CheckCheck, AlertTriangle, XCircle, ShoppingBag, Menu } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const initialNotifications = [
  { id: 1, type: 'cancel', title: 'Order Canceled Alert', desc: 'Order #ORD-9838 was canceled by customer.', time: '10m ago', unread: true },
  { id: 2, type: 'stock', title: 'Low Stock Warning', desc: 'Pizza Mozzarella Cheese dropped below 2,000g threshold.', time: '25m ago', unread: true },
  { id: 3, type: 'order', title: 'New High Value Order', desc: 'Order #ORD-9840 received (PKR 3,200).', time: '45m ago', unread: false },
];

const Header = ({ title = "Dashboard Overview", onMobileMenuToggle }) => {
  const { admin } = useAuth();
  const [notifications, setNotifications] = useState(initialNotifications);
  const [showDropdown, setShowDropdown] = useState(false);
  const dropdownRef = useRef(null);

  const unreadCount = notifications.filter(n => n.unread).length;

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const markAllRead = () => {
    setNotifications(prev => prev.map(n => ({ ...n, unread: false })));
  };

  return (
    <header className="sticky top-0 z-20 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md border-b border-slate-200 dark:border-slate-800 px-4 sm:px-8 py-3.5 flex items-center justify-between transition-colors">
      <div className="flex items-center gap-3">
        {/* Mobile Hamburger Toggle Button */}
        <button
          onClick={onMobileMenuToggle}
          className="md:hidden p-2 rounded-xl text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200"
          aria-label="Toggle Mobile Menu"
        >
          <Menu className="w-5 h-5" />
        </button>

        <div>
          <h2 className="text-lg sm:text-xl font-extrabold text-slate-900 dark:text-white tracking-tight">{title}</h2>
          <p className="hidden sm:block text-xs text-slate-500 dark:text-slate-400 font-medium">Real-Time Operations & Business Affairs Portal</p>
        </div>
      </div>

      <div className="flex items-center gap-3 sm:gap-4">
        {/* Live Status Badge */}
        <div className="hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-full bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800/50 text-emerald-600 dark:text-emerald-400 text-xs font-bold">
          <span className="w-2 h-2 rounded-full bg-emerald-500 animate-ping" />
          <span>Live Systems</span>
        </div>

        {/* Notifications Icon & Dropdown Container */}
        <div className="relative" ref={dropdownRef}>
          <button
            onClick={() => setShowDropdown(!showDropdown)}
            className="relative p-2.5 rounded-xl text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white bg-slate-100 dark:bg-slate-800 transition-colors"
            title="Notifications"
          >
            <Bell className="w-5 h-5" />
            {unreadCount > 0 && (
              <span className="absolute -top-1 -right-1 w-5 h-5 bg-orange-500 text-white font-extrabold text-[10px] rounded-full flex items-center justify-center border-2 border-white dark:border-slate-900">
                {unreadCount}
              </span>
            )}
          </button>

          {/* Interactive Notifications Dropdown */}
          {showDropdown && (
            <div className="absolute right-0 mt-3 w-80 sm:w-96 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl z-50 overflow-hidden space-y-2">
              <div className="p-4 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <h3 className="font-extrabold text-sm text-slate-900 dark:text-white">Activity Alerts</h3>
                  {unreadCount > 0 && (
                    <span className="px-2 py-0.5 text-[10px] font-black bg-orange-100 text-orange-600 dark:bg-orange-950 dark:text-orange-400 rounded-full">
                      {unreadCount} New
                    </span>
                  )}
                </div>
                <button
                  onClick={markAllRead}
                  className="text-xs font-bold text-orange-500 hover:underline flex items-center gap-1"
                >
                  <CheckCheck className="w-3.5 h-3.5" /> Mark all read
                </button>
              </div>

              <div className="max-h-72 overflow-y-auto divide-y divide-slate-100 dark:divide-slate-800/80">
                {notifications.map((n) => (
                  <div
                    key={n.id}
                    className={`p-3.5 flex items-start gap-3 transition-colors ${
                      n.unread ? 'bg-orange-50/50 dark:bg-orange-950/20' : 'hover:bg-slate-50 dark:hover:bg-slate-800/50'
                    }`}
                  >
                    <div className="p-2 rounded-xl mt-0.5 shrink-0">
                      {n.type === 'cancel' && <XCircle className="w-4 h-4 text-rose-500" />}
                      {n.type === 'stock' && <AlertTriangle className="w-4 h-4 text-amber-500" />}
                      {n.type === 'order' && <ShoppingBag className="w-4 h-4 text-emerald-500" />}
                    </div>
                    <div className="flex-1 space-y-0.5">
                      <div className="flex items-center justify-between">
                        <p className="font-bold text-xs text-slate-900 dark:text-white">{n.title}</p>
                        <span className="text-[10px] text-slate-400 font-medium">{n.time}</span>
                      </div>
                      <p className="text-xs text-slate-600 dark:text-slate-400">{n.desc}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* User Profile Avatar Snippet */}
        <div className="flex items-center gap-2 pl-2 border-l border-slate-200 dark:border-slate-800">
          {admin.avatarUrl ? (
            <img src={admin.avatarUrl} alt="Avatar" className="w-9 h-9 rounded-full object-cover ring-2 ring-orange-500/20" />
          ) : (
            <div className="w-9 h-9 rounded-full bg-gradient-to-tr from-orange-500 to-amber-400 text-white flex items-center justify-center font-bold text-sm shadow-md">
              {admin.fullName ? admin.fullName[0] : 'A'}
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;
