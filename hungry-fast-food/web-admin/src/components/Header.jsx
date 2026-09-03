import React, { useState, useRef, useEffect } from 'react';
import { Bell, CheckCheck, AlertTriangle, XCircle, ShoppingBag, Menu, Flame } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const initialNotifications = [
  { id: 1, type: 'cancel', title: 'Order Canceled Alert', desc: 'Order #ORD-9838 was canceled by customer.', time: '10m ago', unread: true },
  { id: 2, type: 'stock', title: 'Low Stock Warning', desc: 'Pizza Mozzarella Cheese dropped below 2,000g threshold.', time: '25m ago', unread: true },
  { id: 3, type: 'order', title: 'New High Value Order', desc: 'Order #ORD-9840 received (PKR 3,200).', time: '45m ago', unread: false },
];

const Header = ({ title = "Dashboard", onMobileMenuToggle }) => {
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
    <header className="sticky top-0 z-20 bg-white/90 dark:bg-slate-900/90 backdrop-blur-md border-b border-slate-200 dark:border-slate-800/80 px-4 sm:px-8 py-3 flex items-center justify-between transition-colors">
      
      {/* Left Section: Mobile Toggle & Page Title */}
      <div className="flex items-center gap-3 truncate">
        <button
          onClick={onMobileMenuToggle}
          className="md:hidden p-2 rounded-xl text-slate-700 dark:text-slate-200 bg-slate-100 dark:bg-slate-800/80 hover:bg-slate-200 transition-colors shrink-0"
          aria-label="Toggle Navigation Menu"
        >
          <Menu className="w-5 h-5" />
        </button>

        <div className="flex items-center gap-2 truncate">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-amber-500 to-orange-600 flex md:hidden items-center justify-center text-white shrink-0">
            <Flame className="w-4 h-4 fill-white" />
          </div>
          <h2 className="text-base sm:text-xl font-extrabold text-slate-900 dark:text-white tracking-tight truncate">
            {title}
          </h2>
        </div>
      </div>

      {/* Right Section: Status, Bell & Avatar */}
      <div className="flex items-center gap-2.5 sm:gap-4 shrink-0">
        {/* Live Systems Badge (Hidden on mobile) */}
        <div className="hidden lg:flex items-center gap-2 px-3 py-1 rounded-full bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800/50 text-emerald-600 dark:text-emerald-400 text-xs font-bold">
          <span className="w-2 h-2 rounded-full bg-emerald-500 animate-ping" />
          <span>Live Systems Online</span>
        </div>

        {/* Notifications Dropdown Container */}
        <div className="relative" ref={dropdownRef}>
          <button
            onClick={() => setShowDropdown(!showDropdown)}
            className="relative p-2 rounded-xl text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white bg-slate-100 dark:bg-slate-800/80 transition-colors"
            aria-label="Notifications"
          >
            <Bell className="w-5 h-5" />
            {unreadCount > 0 && (
              <span className="absolute -top-1 -right-1 w-4 h-4 bg-orange-500 text-white font-black text-[9px] rounded-full flex items-center justify-center border-2 border-white dark:border-slate-900">
                {unreadCount}
              </span>
            )}
          </button>

          {/* Notifications Dropdown */}
          {showDropdown && (
            <div className="absolute right-0 mt-3 w-72 sm:w-88 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl z-50 overflow-hidden space-y-1">
              <div className="p-3.5 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between bg-slate-50/50 dark:bg-slate-800/30">
                <div className="flex items-center gap-1.5">
                  <h3 className="font-extrabold text-xs text-slate-900 dark:text-white uppercase tracking-wider">Alerts</h3>
                  {unreadCount > 0 && (
                    <span className="px-1.5 py-0.5 text-[9px] font-black bg-orange-100 text-orange-600 dark:bg-orange-950 dark:text-orange-400 rounded-full">
                      {unreadCount}
                    </span>
                  )}
                </div>
                <button
                  onClick={markAllRead}
                  className="text-[11px] font-bold text-orange-500 hover:underline flex items-center gap-1"
                >
                  <CheckCheck className="w-3.5 h-3.5" /> Read all
                </button>
              </div>

              <div className="max-h-64 overflow-y-auto divide-y divide-slate-100 dark:divide-slate-800/80">
                {notifications.map((n) => (
                  <div
                    key={n.id}
                    className={`p-3 flex items-start gap-2.5 transition-colors ${
                      n.unread ? 'bg-orange-50/40 dark:bg-orange-950/20' : 'hover:bg-slate-50 dark:hover:bg-slate-800/40'
                    }`}
                  >
                    <div className="p-1.5 rounded-lg shrink-0 mt-0.5">
                      {n.type === 'cancel' && <XCircle className="w-4 h-4 text-rose-500" />}
                      {n.type === 'stock' && <AlertTriangle className="w-4 h-4 text-amber-500" />}
                      {n.type === 'order' && <ShoppingBag className="w-4 h-4 text-emerald-500" />}
                    </div>
                    <div className="flex-1 space-y-0.5 min-w-0">
                      <div className="flex items-center justify-between">
                        <p className="font-bold text-xs text-slate-900 dark:text-white truncate">{n.title}</p>
                        <span className="text-[9px] text-slate-400 font-semibold shrink-0 ml-1">{n.time}</span>
                      </div>
                      <p className="text-[11px] text-slate-600 dark:text-slate-400 line-clamp-2">{n.desc}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* User Profile Avatar */}
        <div className="flex items-center gap-2 pl-2 border-l border-slate-200 dark:border-slate-800">
          {admin.avatarUrl ? (
            <img src={admin.avatarUrl} alt="Avatar" className="w-8 h-8 rounded-full object-cover ring-2 ring-orange-500/20" />
          ) : (
            <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-orange-500 to-amber-400 text-white flex items-center justify-center font-black text-xs shadow-md">
              {admin.fullName ? admin.fullName[0] : 'A'}
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;
