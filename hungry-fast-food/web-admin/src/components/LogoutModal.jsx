import React from 'react';
import { LogOut, X, AlertCircle } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const LogoutModal = () => {
  const { showLogoutModal, setShowLogoutModal, confirmLogout } = useAuth();
  const navigate = useNavigate();

  if (!showLogoutModal) return null;

  const handleConfirm = () => {
    confirmLogout();
    navigate('/login');
  };

  return (
    <div className="fixed inset-0 z-50 bg-slate-950/70 backdrop-blur-sm flex items-center justify-center p-4">
      <div className="w-full max-w-md p-6 rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl space-y-6 animate-in fade-in zoom-in duration-150">
        
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-2xl bg-rose-50 dark:bg-rose-950/50 text-rose-600 dark:text-rose-400">
              <LogOut className="w-5 h-5" />
            </div>
            <div>
              <h3 className="text-lg font-extrabold text-slate-900 dark:text-white">Confirm Sign Out</h3>
              <p className="text-xs text-slate-500 dark:text-slate-400">Web Admin Session Management</p>
            </div>
          </div>
          <button
            onClick={() => setShowLogoutModal(false)}
            className="p-1 rounded-lg text-slate-400 hover:text-slate-600 dark:hover:text-white"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Message */}
        <p className="text-sm font-medium text-slate-600 dark:text-slate-300 leading-relaxed">
          Are you sure you want to log out of the Hungry Hub Web Admin Control Panel? Your active JWT session token will be invalidated.
        </p>

        {/* Action Buttons */}
        <div className="flex items-center gap-3 pt-2">
          <button
            onClick={() => setShowLogoutModal(false)}
            className="w-1/2 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 font-bold text-xs hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleConfirm}
            className="w-1/2 py-2.5 rounded-xl bg-rose-600 text-white font-extrabold text-xs shadow-lg shadow-rose-600/25 hover:bg-rose-700 transition-colors flex items-center justify-center gap-1.5"
          >
            <LogOut className="w-4 h-4" /> Yes, Log Out
          </button>
        </div>
      </div>
    </div>
  );
};

export default LogoutModal;
